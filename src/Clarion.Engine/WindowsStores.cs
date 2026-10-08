using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Clarion.Core.Abstractions;
using Clarion.Core.Appx;
using Clarion.Core.Features;
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
    private AppxSnapshot? _cache;

    public AppxSnapshot GetSnapshot()
    {
        if (_cache is not null) return _cache;
        var result = Run(AppxScripts.Inventory());
        _cache = AppxSnapshot.Parse(result.Output);
        return _cache;
    }

    public void Remove(string name, bool allUsers, bool deprovision)
    {
        _cache = null;
        Run(AppxScripts.Remove(name, allUsers, deprovision));
        _cache = null;
    }

    public void Restore(string familyName)
    {
        _cache = null;
        try
        {
            Run(AppxScripts.Restore(familyName));
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidOperationException($"{ex.Message} If Windows no longer has the package files, reinstall the app from the Microsoft Store.");
        }
        _cache = null;
    }

    private ProcessResult Run(string script)
    {
        var result = runner.Run("powershell.exe", AppxScripts.ToArguments(script), Timeout);
        if (result.ExitCode != 0)
        {
            var first = (string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            throw new InvalidOperationException(first ?? "PowerShell command failed.");
        }
        return result;
    }
}

[SupportedOSPlatform("windows")]
public sealed class WindowsFeatureStore(IProcessRunner runner) : IFeatureStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(30);
    private Dictionary<string, string>? _features;
    private readonly Dictionary<string, string?> _capabilities = new(StringComparer.OrdinalIgnoreCase);

    public bool? IsFeatureEnabled(string name)
    {
        _features ??= FeatureScripts.ParseStates(Run(FeatureScripts.Inventory()), "features", "FeatureName");
        return _features.TryGetValue(name, out var state) ? FeatureRules.IsEnabledState(state) : null;
    }

    public void SetFeature(string name, bool enabled)
    {
        Run(FeatureScripts.SetFeature(name, enabled));
        _features = null;
    }

    public bool? IsCapabilityInstalled(string name)
    {
        if (!_capabilities.TryGetValue(name, out var state))
        {
            var map = FeatureScripts.ParseStates(Run(FeatureScripts.CapabilityState(name)), "capabilities", "Name");
            state = map.Count == 0 ? null : map.Values.First();
            _capabilities[name] = state;
        }
        return state is null ? null : FeatureRules.IsInstalledState(state);
    }

    public void SetCapability(string name, bool installed)
    {
        Run(FeatureScripts.SetCapability(name, installed));
        _capabilities.Remove(name);
    }

    private string Run(string script)
    {
        var result = runner.Run("powershell.exe", FeatureScripts.ToArguments(script), Timeout);
        if (result.ExitCode != 0)
        {
            var first = (string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Error)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
            throw new InvalidOperationException(first ?? "PowerShell command failed.");
        }
        return result.Output;
    }
}
