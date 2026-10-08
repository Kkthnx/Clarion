using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Clarion.Core.Abstractions;
using Clarion.Core.Appx;
using Clarion.Core.Dns;
using Clarion.Core.Engine;
using System.Text.Json;
using Clarion.Core.Features;
using Clarion.Core.Power;
using Clarion.Core.Model;
using Microsoft.Win32;

namespace Clarion.Engine;

[SupportedOSPlatform("windows")]
public sealed class WindowsProcessRunner : IProcessRunner
{
    public ProcessResult Run(string fileName, string arguments, TimeSpan timeout)
    {
        var psi = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Could not start {fileName}");
        var stdout = p.StandardOutput.ReadToEndAsync();
        var stderr = p.StandardError.ReadToEndAsync();
        if (!p.WaitForExit(timeout))
        {
            try { p.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
            return new ProcessResult(-1, "", $"{fileName} timed out");
        }
        return new ProcessResult(p.ExitCode, stdout.Result, stderr.Result);
    }
}

/// <summary>
/// Reads start type from the service registry key and writes it through sc.exe.
/// Driver and boot or system start services are reported as missing so they are never touched.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsServiceStore(IProcessRunner runner) : IServiceStore
{
    private const string Root = "SYSTEM\\CurrentControlSet\\Services\\";

    public ServiceStartType? GetStartType(string name)
    {
        using var key = Registry.LocalMachine.OpenSubKey(Root + name);
        if (key?.GetValue("Start") is not int start) return null;
        var type = key.GetValue("Type") is int t ? t : 0;
        if ((type & 0xF) != 0) return null;
        return start switch
        {
            2 => key.GetValue("DelayedAutostart") is int d && d == 1 ? ServiceStartType.AutomaticDelayed : ServiceStartType.Automatic,
            3 => ServiceStartType.Manual,
            4 => ServiceStartType.Disabled,
            _ => null,
        };
    }

    public void SetStartType(string name, ServiceStartType startType)
    {
        var mode = startType switch
        {
            ServiceStartType.Automatic => "auto",
            ServiceStartType.AutomaticDelayed => "delayed-auto",
            ServiceStartType.Manual => "demand",
            _ => "disabled",
        };
        var result = runner.Run("sc.exe", $"config {name} start= {mode}", TimeSpan.FromSeconds(30));
        if (result.ExitCode != 0) throw new InvalidOperationException($"sc config failed: {result.Output.Trim()}");
        if (startType == ServiceStartType.Disabled) runner.Run("sc.exe", $"stop {name}", TimeSpan.FromSeconds(30));
    }
}

[SupportedOSPlatform("windows")]
public sealed class WindowsTaskStore : ITaskStore
{
    public bool? GetEnabled(string path)
    {
        try
        {
            dynamic task = GetTask(path);
            return (bool)task.Enabled;
        }
        catch (Exception ex) when (ex is COMException or FileNotFoundException)
        {
            return null;
        }
    }

    public void SetEnabled(string path, bool enabled)
    {
        dynamic task = GetTask(path);
        task.Enabled = enabled;
    }

    private static dynamic GetTask(string path)
    {
        var type = Type.GetTypeFromProgID("Schedule.Service") ?? throw new InvalidOperationException("Task Scheduler is not available");
        dynamic service = Activator.CreateInstance(type)!;
        service.Connect();
        var split = path.LastIndexOf('\\');
        var folder = split <= 0 ? "\\" : path[..split];
        dynamic f = service.GetFolder(folder);
        return f.GetTask(path[(split + 1)..]);
    }
}

[SupportedOSPlatform("windows")]
public static class WindowsMachine
{
    public static bool IsElevated()
    {
        using var id = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static MachineProfile Detect()
    {
        using var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion");
        var edition = key?.GetValue("EditionID") as string ?? "Unknown";
        return new MachineProfile(Environment.OSVersion.Version.Build, edition);
    }
}

[SupportedOSPlatform("windows")]
public sealed class WindowsAppxStore(IProcessRunner runner) : IAppxStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
    private readonly object _gate = new();
    private AppxSnapshot? _cache;

    public void Invalidate()
    {
        lock (_gate) _cache = null;
    }

    public AppxSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            return _cache ??= AppxSnapshot.Parse(Run(AppxScripts.Inventory()));
        }
    }

    public void Remove(string name, bool allUsers, bool deprovision)
    {
        lock (_gate) _cache = null;
        Run(AppxScripts.Remove(name, allUsers, deprovision));
        lock (_gate) _cache = null;
    }

    public void Restore(string familyName)
    {
        lock (_gate) _cache = null;
        try
        {
            Run(AppxScripts.Restore(familyName));
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"{ex.Message} If Windows no longer has the package files, reinstall the app from the Microsoft Store.");
        }
        lock (_gate) _cache = null;
    }

    private string Run(string script) => PowerShellHost.Run(runner, script, Timeout);
}

[SupportedOSPlatform("windows")]
public sealed class WindowsFeatureStore(IProcessRunner runner) : IFeatureStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(30);
    private readonly object _gate = new();
    private Dictionary<string, string>? _features;
    private readonly Dictionary<string, string?> _capabilities = new(StringComparer.OrdinalIgnoreCase);

    public void Prefetch() => LoadFeatures();

    public void Invalidate()
    {
        lock (_gate)
        {
            _features = null;
            _capabilities.Clear();
        }
    }

    private Dictionary<string, string> LoadFeatures()
    {
        lock (_gate)
        {
            return _features ??= FeatureScripts.ParseStates(Run(FeatureScripts.Inventory()), "features", "FeatureName");
        }
    }

    public bool? IsFeatureEnabled(string name) =>
        LoadFeatures().TryGetValue(name, out var state) ? FeatureRules.IsEnabledState(state) : null;

    public void SetFeature(string name, bool enabled)
    {
        Run(FeatureScripts.SetFeature(name, enabled));
        lock (_gate) _features = null;
    }

    public bool? IsCapabilityInstalled(string name)
    {
        string? state;
        lock (_gate)
        {
            if (_capabilities.TryGetValue(name, out state)) return state is null ? null : FeatureRules.IsInstalledState(state);
        }
        var map = FeatureScripts.ParseStates(Run(FeatureScripts.CapabilityState(name)), "capabilities", "Name");
        state = map.Count == 0 ? null : map.Values.First();
        lock (_gate) _capabilities[name] = state;
        return state is null ? null : FeatureRules.IsInstalledState(state);
    }

    public void SetCapability(string name, bool installed)
    {
        Run(FeatureScripts.SetCapability(name, installed));
        lock (_gate) _capabilities.Remove(name);
    }

    private string Run(string script) => PowerShellHost.Run(runner, script, Timeout);
}

[SupportedOSPlatform("windows")]
public sealed class WindowsPowerStore(IProcessRunner runner) : IPowerStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public IReadOnlyList<PowerPlan> List()
    {
        var result = runner.Run("powercfg.exe", "/list", Timeout);
        if (result.ExitCode != 0) throw new InvalidOperationException("Could not read power plans.");
        return PowerPlans.ParseList(result.Output);
    }

    public void SetActive(string guid) => Run($"/setactive {Guid(guid)}");

    public string Duplicate(string templateGuid)
    {
        var result = Run($"/duplicatescheme {Guid(templateGuid)}");
        var created = PowerPlans.ParseList(result.Output).FirstOrDefault()
                      ?? throw new InvalidOperationException("Windows did not create the power plan. Some PCs do not offer it.");
        return created.Guid;
    }

    public void Rename(string guid, string name) => Run($"/changename {Guid(guid)} \"{name.Replace("\"", "")}\"");

    public bool? IsHibernationEnabled()
    {
        using var key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Power");
        return key?.GetValue("HibernateEnabled") is int v ? v != 0 : null;
    }

    public void SetHibernation(bool enabled) => Run($"/hibernate {(enabled ? "on" : "off")}");

    private ProcessResult Run(string args)
    {
        var result = runner.Run("powercfg.exe", args, Timeout);
        if (result.ExitCode != 0) throw new InvalidOperationException(PowerShellHost.FirstLine(result.Error, result.Output, "powercfg failed."));
        return result;
    }

    private static string Guid(string value) =>
        PowerPlans.IsGuid(value) ? value : throw new ArgumentException($"Not a GUID: {value}");
}

[SupportedOSPlatform("windows")]
public sealed class WindowsStreamingRunner : Clarion.Core.Actions.IStreamingRunner
{
    static WindowsStreamingRunner()
    {
        // Windows tools write in the console code page, which .NET only knows after this is registered.
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    public async Task<int> RunAsync(string systemTool, string args, bool utf16, Action<string> onLine, CancellationToken cancel)
    {
        // The tool is always taken from the System32 folder, never from the search path.
        var path = Path.Combine(Environment.SystemDirectory, systemTool);
        var psi = new ProcessStartInfo(path, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = utf16 ? System.Text.Encoding.Unicode : System.Text.Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
            StandardErrorEncoding = utf16 ? System.Text.Encoding.Unicode : System.Text.Encoding.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage),
        };
        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Could not start {systemTool}");
        using var registration = cancel.Register(() =>
        {
            try { process.Kill(entireProcessTree: true); } catch (InvalidOperationException) { }
        });

        var readers = new[] { Pump(process.StandardOutput, onLine), Pump(process.StandardError, onLine) };
        await process.WaitForExitAsync(CancellationToken.None);
        await Task.WhenAll(readers);
        cancel.ThrowIfCancellationRequested();
        return process.ExitCode;
    }

    /// <summary>Splits on line feeds and carriage returns so progress lines that overwrite themselves still show.</summary>
    private static async Task Pump(StreamReader reader, Action<string> onLine)
    {
        var line = new System.Text.StringBuilder();
        var buffer = new char[512];
        int read;
        while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            for (var i = 0; i < read; i++)
            {
                var ch = buffer[i];
                if (ch is '\n' or '\r')
                {
                    Flush(line, onLine);
                }
                else if (ch != '\0')
                {
                    line.Append(ch);
                }
            }
        }
        Flush(line, onLine);
    }

    private static void Flush(System.Text.StringBuilder line, Action<string> onLine)
    {
        var text = line.ToString().Trim();
        line.Clear();
        if (text.Length > 0) onLine(text);
    }
}

[SupportedOSPlatform("windows")]
public sealed class WindowsDnsStore(IProcessRunner runner) : IDnsStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    public IReadOnlyList<AdapterDns> GetAdapters()
    {
        const string script =
            "$a=@(Get-NetAdapter | Where-Object { $_.Status -eq 'Up' -and $_.HardwareInterface } | ForEach-Object { " +
            "$s=@((Get-DnsClientServerAddress -InterfaceIndex $_.InterfaceIndex -AddressFamily IPv4).ServerAddresses); " +
            "[pscustomobject]@{IfIndex=[int]$_.InterfaceIndex;Alias=$_.Name;Servers=$s} }); " +
            "ConvertTo-Json -InputObject @{adapters=$a} -Depth 4 -Compress";
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(PowerShellHost.Run(runner, script, Timeout));
        }
        catch (InvalidOperationException)
        {
            return []; // no network cmdlets or no adapters, so there is nothing to change
        }
        using var _ = doc;
        return doc.RootElement.GetProperty("adapters").EnumerateArray().Select(e => new AdapterDns(
            e.GetProperty("IfIndex").GetInt32(),
            e.GetProperty("Alias").GetString() ?? "",
            e.GetProperty("Servers").ValueKind == JsonValueKind.Array
                ? e.GetProperty("Servers").EnumerateArray().Select(x => x.GetString() ?? "").Where(DnsProviders.IsIpv4).ToList()
                : [])).ToList();
    }

    public void SetServers(int ifIndex, IReadOnlyList<string> servers)
    {
        if (!servers.All(DnsProviders.IsIpv4)) throw new ArgumentException("DNS servers must be IPv4 addresses.");
        PowerShellHost.Run(runner, $"Set-DnsClientServerAddress -InterfaceIndex {ifIndex} -ServerAddresses ({string.Join(",", servers.Select(s => $"'{s}'"))})", Timeout);
    }

    public void ResetServers(int ifIndex) =>
        PowerShellHost.Run(runner, $"Set-DnsClientServerAddress -InterfaceIndex {ifIndex} -ResetServerAddresses", Timeout);

    public IReadOnlyList<DohRegistration> GetDohRegistrations()
    {
        const string script =
            "$d=@(Get-DnsClientDohServerAddress | ForEach-Object { [pscustomobject]@{Address=$_.ServerAddress;Template=$_.DohTemplate;Auto=[bool]$_.AutoUpgrade;Fallback=[bool]$_.AllowFallbackToUdp} }); " +
            "ConvertTo-Json -InputObject @{doh=$d} -Depth 4 -Compress";
        try
        {
            using var doc = JsonDocument.Parse(PowerShellHost.Run(runner, script, Timeout));
            return doc.RootElement.GetProperty("doh").EnumerateArray().Select(e => new DohRegistration(
                e.GetProperty("Address").GetString() ?? "", e.GetProperty("Template").GetString() ?? "",
                e.GetProperty("Auto").GetBoolean(), e.GetProperty("Fallback").GetBoolean()))
                .Where(r => DnsProviders.IsIpv4(r.Address)).ToList();
        }
        catch (InvalidOperationException)
        {
            return []; // Windows 10 has no encrypted DNS client
        }
    }

    public void UpsertDoh(DohRegistration r)
    {
        if (!DnsProviders.IsIpv4(r.Address) || !Uri.TryCreate(r.Template, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("Not a valid encrypted DNS entry.");
        var known = GetDohRegistrations().Any(x => x.Address == r.Address);
        var verb = known ? "Set" : "Add";
        PowerShellHost.Run(runner,
            $"{verb}-DnsClientDohServerAddress -ServerAddress '{r.Address}' -DohTemplate '{r.Template}' -AutoUpgrade ${r.AutoUpgrade} -AllowFallbackToUdp ${r.AllowFallbackToUdp}", Timeout);
    }

    public void RemoveDoh(string address)
    {
        if (!DnsProviders.IsIpv4(address)) throw new ArgumentException("Not an IPv4 address.");
        PowerShellHost.Run(runner, $"Remove-DnsClientDohServerAddress -ServerAddress '{address}'", Timeout);
    }
}
