using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Clarion.Core.Abstractions;
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
