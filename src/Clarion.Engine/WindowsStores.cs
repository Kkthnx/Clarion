using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;
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
    public ProcessResult Run(string fileName, string arguments, TimeSpan timeout) => Run(fileName, arguments, timeout, CancellationToken.None);

    public ProcessResult Run(string fileName, string arguments, TimeSpan timeout, CancellationToken cancel)
    {
        // By full path, so a program of the same name beside Clarion or in the current folder is never the one that runs.
        var psi = new ProcessStartInfo(Clarion.Core.Engine.SystemTools.Resolve(fileName, Environment.SystemDirectory), arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        // winget writes UTF-8 to a pipe (a registered sign shows as the bytes C2 AE), and a program with no console has no code page to
        // fall back on, so a publisher name such as HWiNFO with a registered sign would come back garbled.
        if (Path.GetFileName(fileName).Equals("winget.exe", StringComparison.OrdinalIgnoreCase))
        {
            psi.StandardOutputEncoding = new UTF8Encoding(false);
            psi.StandardErrorEncoding = new UTF8Encoding(false);
        }
        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Could not start {fileName}");
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        var pumps = new[] { Pump(p.StandardOutput, stdout), Pump(p.StandardError, stderr) };
        // Waited for in short steps, so a stop request is noticed within a fifth of a second.
        var clock = Stopwatch.StartNew();
        while (!p.WaitForExit(200))
        {
            var stopped = cancel.IsCancellationRequested;
            if (!stopped && (timeout < TimeSpan.Zero || clock.Elapsed < timeout)) continue;
            // A child that Windows will not let Clarion end must not turn a stop into a crash. The tree is given a moment to go, so a retry
            // right away does not collide with it.
            try { p.Kill(entireProcessTree: true); p.WaitForExit(3000); } catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception) { }
            return stopped ? new ProcessResult(-2, "", $"{fileName} was stopped") : new ProcessResult(-1, "", $"{fileName} timed out");
        }
        // A helper the program left running keeps both pipes open after the program itself has exited. Waiting for the pipes to close
        // would then block until that helper ends, so the output gathered so far is taken after a short grace period instead.
        Task.WaitAll(pumps, PipeGrace);
        return new ProcessResult(p.ExitCode, Read(stdout), Read(stderr));
    }

    private static readonly TimeSpan PipeGrace = TimeSpan.FromSeconds(2);

    // The pipes Windows gives a child process are synchronous, so a read holds its thread until data arrives. Each pipe gets a thread of
    // its own: on a busy or small machine the shared pool can be full, and the read would then not even start before the grace period ran out.
    private static Task Pump(StreamReader reader, StringBuilder into) =>
        Task.Factory.StartNew(() =>
        {
            var buffer = new char[4096];
            try
            {
                int count;
                while ((count = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    lock (into) into.Append(buffer, 0, count);
                }
            }
            catch (ObjectDisposedException) { }
            catch (IOException) { }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    private static string Read(StringBuilder text)
    {
        lock (text) return text.ToString();
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
        // A name with a backslash would open some other key under CurrentControlSet, so it is not a service.
        if (!ServiceNames.IsValid(name)) return null;
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
        // The name goes onto the sc.exe command line, where a space would add arguments such as a different program to run.
        if (!ServiceNames.IsValid(name)) throw new ArgumentException($"Not a valid service name: {name}");
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
        return new MachineProfile(Environment.OSVersion.Version.Build, edition) { Traits = WindowsMachineTraits.Detect() };
    }
}

[SupportedOSPlatform("windows")]
public sealed class WindowsAppxStore(IProcessRunner runner) : IAppxStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);
    private readonly object _gate = new();
    private AppxSnapshot? _cache;
    private string? _failure;

    public void Invalidate()
    {
        lock (_gate) { _cache = null; _failure = null; }
    }

    /// <summary>
    /// A failed read is remembered until the next refresh. Every app setting asks for this list, and without administrator rights each
    /// ask would run PowerShell again just to fail the same way, which kept the whole read busy for minutes.
    /// </summary>
    public AppxSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            if (_cache is not null) return _cache;
            if (_failure is not null) throw new InvalidOperationException(_failure);
            try { return _cache = AppxSnapshot.Parse(Run(AppxScripts.Inventory())); }
            catch (InvalidOperationException ex) { _failure = ex.Message; throw; }
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
/// <param name="runner">Runs PowerShell, which is used when the DISM API cannot be, and for every change.</param>
/// <param name="fastReads">False makes every read go through PowerShell, which is what a test with a fake runner needs.</param>
public sealed class WindowsFeatureStore(IProcessRunner runner, bool fastReads = true) : IFeatureStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(30);
    private readonly object _gate = new();
    // A null state means this PC does not have the feature, which is remembered too so it is not asked again.
    private readonly Dictionary<string, string?> _features = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string?> _capabilities = new(StringComparer.OrdinalIgnoreCase);
    private string? _featureFailure;

    public void Prefetch(IReadOnlyList<string> featureNames) => LoadFeatures(featureNames);

    public void Invalidate()
    {
        lock (_gate)
        {
            _features.Clear();
            _capabilities.Clear();
            _featureFailure = null;
        }
    }

    /// <summary>Reads the states of the names not known yet, all in one PowerShell run.</summary>
    private void LoadFeatures(IEnumerable<string> names)
    {
        lock (_gate)
        {
            var missing = names.Where(n => !_features.ContainsKey(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (missing.Count == 0) return;
            // A read that failed, for example because the rights are missing, is remembered until the next refresh. Otherwise each
            // setting would run PowerShell again to fail the same way.
            if (_featureFailure is not null) throw new InvalidOperationException(_featureFailure);
            // The DISM API answers in a fraction of the time PowerShell needs just to start. When it cannot be used, PowerShell asks as before.
            var found = fastReads && missing.All(FeatureRules.IsValidName) ? WindowsDism.ReadFeatures(missing) : null;
            if (found is null)
            {
                try { found = FeatureScripts.ParseStates(Run(FeatureScripts.FeatureStates(missing)), "features", "FeatureName"); }
                catch (InvalidOperationException ex) { _featureFailure = ex.Message; throw; }
            }
            foreach (var n in missing) _features[n] = found.TryGetValue(n, out var state) ? state : null;
        }
    }

    public bool? IsFeatureEnabled(string name)
    {
        LoadFeatures([name]);
        lock (_gate) return _features[name] is { } state ? FeatureRules.IsEnabledState(state) : null;
    }

    public void SetFeature(string name, bool enabled)
    {
        Run(FeatureScripts.SetFeature(name, enabled));
        // Turning one feature on can turn on the ones it needs, so every cached state is dropped.
        lock (_gate) { _features.Clear(); _featureFailure = null; }
    }

    public bool? IsCapabilityInstalled(string name)
    {
        string? state;
        lock (_gate)
        {
            if (_capabilities.TryGetValue(name, out state)) return state is null ? null : FeatureRules.IsInstalledState(state);
        }
        state = fastReads && FeatureRules.IsValidName(name) ? WindowsDism.ReadCapability(name) : null;
        if (state is null)
        {
            var map = FeatureScripts.ParseStates(Run(FeatureScripts.CapabilityState(name)), "capabilities", "Name");
            state = map.Count == 0 ? null : map.Values.First();
        }
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

        // Each pipe is read on a thread of its own. On a busy machine the thread pool can take longer than the grace period below to start a
        // queued reader, and the output of a tool that had already finished would then be dropped.
        var readers = Task.WhenAll(Pump(process.StandardOutput, onLine), Pump(process.StandardError, onLine));
        await process.WaitForExitAsync(CancellationToken.None);
        // A tool can start a helper and exit, and the helper then keeps both pipes open. The job is over when the tool is, so the
        // rest of its output gets a short grace period instead of holding the screen on "Running" until the helper ends.
        await Task.WhenAny(readers, Task.Delay(TimeSpan.FromSeconds(2), CancellationToken.None));
        cancel.ThrowIfCancellationRequested();
        return process.ExitCode;
    }

    /// <summary>Splits on line feeds and carriage returns so progress lines that overwrite themselves still show.</summary>
    private static Task Pump(StreamReader reader, Action<string> onLine) =>
        Task.Factory.StartNew(() => PumpBlocking(reader, onLine), CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

    private static void PumpBlocking(StreamReader reader, Action<string> onLine)
    {
        var line = new System.Text.StringBuilder();
        var buffer = new char[512];
        int read;
        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
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
/// <param name="fastReads">False makes every read go through PowerShell, which is what a test with a fake runner needs.</param>
public sealed class WindowsDnsStore(IProcessRunner runner, bool fastReads = true) : IDnsStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    // Only the servers set by hand count. Get-DnsClientServerAddress also lists the ones the router hands out, and writing
    // those back as typed in servers would turn an automatic adapter into a fixed one. Empty means automatic.
    private const string StaticServersScript =
        "$ns=(Get-ItemProperty ('HKLM:\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\' + $_.InterfaceGuid) -ErrorAction SilentlyContinue).NameServer; " +
        "$s=@(); if ($ns) { $s=@($ns -split '[,\\s]+' | Where-Object { $_ }) }; ";

    public IReadOnlyList<AdapterDns> GetAdapters()
    {
        if (fastReads && WindowsNetworkCim.Adapters() is { } fast) return fast;
        const string script =
            "$a=@(Get-NetAdapter | Where-Object { $_.Status -eq 'Up' -and $_.HardwareInterface } | ForEach-Object { " +
            StaticServersScript +
            "[pscustomobject]@{IfIndex=[int]$_.InterfaceIndex;Alias=$_.Name;Servers=$s} }); " +
            "ConvertTo-Json -InputObject @{adapters=$a} -Depth 4 -Compress";
        try
        {
            using var doc = JsonDocument.Parse(PowerShellHost.Run(runner, script, Timeout));
            return ReadAdapters(doc);
        }
        catch (Exception ex) when (ex is InvalidOperationException or JsonException or KeyNotFoundException)
        {
            return []; // no network cmdlets or no adapters, so there is nothing to change
        }
    }

    private static List<AdapterDns> ReadAdapters(JsonDocument doc) =>
        doc.RootElement.GetProperty("adapters").EnumerateArray().Select(e => new AdapterDns(
            e.GetProperty("IfIndex").GetInt32(),
            e.GetProperty("Alias").GetString() ?? "",
            e.GetProperty("Servers").ValueKind == JsonValueKind.Array
                ? e.GetProperty("Servers").EnumerateArray().Select(x => x.GetString() ?? "").Where(DnsProviders.IsIpv4).ToList()
                : [])).ToList();

    public AdapterDns? Find(int ifIndex)
    {
        if (fastReads && WindowsNetworkCim.Adapters(ifIndex) is { } fast) return fast.FirstOrDefault();
        var script =
            $"$a=@(Get-NetAdapter -InterfaceIndex {ifIndex} -ErrorAction SilentlyContinue | ForEach-Object {{ " +
            StaticServersScript +
            "[pscustomobject]@{IfIndex=[int]$_.InterfaceIndex;Alias=$_.Name;Servers=$s} }); " +
            "ConvertTo-Json -InputObject @{adapters=$a} -Depth 4 -Compress";
        try
        {
            using var doc = JsonDocument.Parse(PowerShellHost.Run(runner, script, Timeout));
            return ReadAdapters(doc).FirstOrDefault();
        }
        catch (Exception ex) when (ex is InvalidOperationException or JsonException or KeyNotFoundException)
        {
            return null; // the adapter is gone, or there are no network cmdlets
        }
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
        if (fastReads && WindowsNetworkCim.DohRegistrations() is { } fast) return fast;
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
        catch (Exception ex) when (ex is InvalidOperationException or JsonException or KeyNotFoundException)
        {
            return []; // Windows 10 has no encrypted DNS client
        }
    }

    public void UpsertDoh(DohRegistration r)
    {
        // The template goes inside single quotes in a PowerShell command, and a web address may legally contain a quote, so one is refused.
        // No real encrypted DNS template needs it.
        if (!DnsProviders.IsIpv4(r.Address) || r.Template.Contains('\'') || !Uri.TryCreate(r.Template, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
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
