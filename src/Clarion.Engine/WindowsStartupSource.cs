using System.Diagnostics;
using System.Runtime.Versioning;
using Clarion.Core.Startup;
using Microsoft.Win32;

namespace Clarion.Engine;

/// <summary>Reads what Windows starts at sign in: the Run keys of the registry and the two Startup folders. Nothing is written.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsStartupSource : IStartupSource
{
    private const string RunKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";

    private readonly Dictionary<string, StartupProgram> _described = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<RawStartupItem> Items()
    {
        var items = new List<RawStartupItem>();
        ReadRun(items, RegistryHive.CurrentUser, RegistryView.Registry64, StartupOrigin.UserRun);
        ReadRun(items, RegistryHive.LocalMachine, RegistryView.Registry64, StartupOrigin.MachineRun);
        ReadRun(items, RegistryHive.LocalMachine, RegistryView.Registry32, StartupOrigin.MachineRun32);
        ReadFolder(items, Environment.GetFolderPath(Environment.SpecialFolder.Startup), StartupOrigin.UserFolder);
        ReadFolder(items, Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), StartupOrigin.MachineFolder);
        return items;
    }

    private static void ReadRun(List<RawStartupItem> into, RegistryHive hive, RegistryView view, StartupOrigin origin)
    {
        try
        {
            using var root = RegistryKey.OpenBaseKey(hive, view);
            using var key = root.OpenSubKey(RunKey);
            if (key is null) return;
            foreach (var name in key.GetValueNames())
            {
                // The default value has no name and is not a program.
                if (name.Length == 0) continue;
                if (key.GetValue(name, null, RegistryValueOptions.DoNotExpandEnvironmentNames) is string command && command.Trim().Length > 0)
                    into.Add(new RawStartupItem(origin, name, command));
            }
        }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
            // A key that cannot be read is left out, and the rest of the list is still shown.
        }
    }

    private static void ReadFolder(List<RawStartupItem> into, string folder, StartupOrigin origin)
    {
        try
        {
            if (!Directory.Exists(folder)) return;
            foreach (var file in Directory.EnumerateFiles(folder))
            {
                var name = Path.GetFileName(file);
                if (name.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase)) continue;
                into.Add(new RawStartupItem(origin, name, file));
            }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
        }
    }

    public byte[]? Approval(StartupOrigin origin, string name)
    {
        try
        {
            var hive = origin is StartupOrigin.UserRun or StartupOrigin.UserFolder ? RegistryHive.CurrentUser : RegistryHive.LocalMachine;
            using var root = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
            using var key = root.OpenSubKey(StartupApproval.KeyOf(origin));
            return key?.GetValue(name) as byte[];
        }
        catch (Exception ex) when (ex is System.Security.SecurityException or UnauthorizedAccessException or IOException)
        {
            return null;
        }
    }

    public StartupProgram Describe(string command)
    {
        if (_described.TryGetValue(command, out var known)) return known;
        var described = DescribeCore(command);
        _described[command] = described;
        return described;
    }

    private static StartupProgram DescribeCore(string command)
    {
        var path = command.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase) ? ShortcutTarget(command) : StartupCommand.ProgramPath(command);
        if (string.IsNullOrWhiteSpace(path)) return new StartupProgram(null, false, "", "");
        try
        {
            // A bare name such as rundll32.exe is found by Windows on its own search path. Only the Windows folders are tried here, and a name
            // that is not in them is not called missing, since the rest of the path is not known.
            if (path.IndexOfAny(['\\', '/']) < 0)
            {
                var inWindows = new[] { Environment.SystemDirectory, Environment.GetFolderPath(Environment.SpecialFolder.Windows) }
                    .Select(d => Path.Combine(d, path)).FirstOrDefault(File.Exists);
                if (inWindows is null) return new StartupProgram(path, true, "", "");
                path = inWindows;
            }
            path = Path.GetFullPath(path);
            if (!File.Exists(path)) return new StartupProgram(path, false, "", "");
            var info = FileVersionInfo.GetVersionInfo(path);
            var description = !string.IsNullOrWhiteSpace(info.FileDescription) ? info.FileDescription : info.ProductName ?? "";
            return new StartupProgram(path, true, (info.CompanyName ?? "").Trim(), description.Trim());
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return new StartupProgram(path, false, "", "");
        }
    }

    /// <summary>The program a shortcut opens, read through the Windows scripting host, or null when it cannot be read.</summary>
    private static string? ShortcutTarget(string shortcut)
    {
        try
        {
            var type = Type.GetTypeFromProgID("WScript.Shell");
            if (type is null) return null;
            dynamic shell = Activator.CreateInstance(type)!;
            try
            {
                dynamic link = shell.CreateShortcut(shortcut);
                return (string?)link.TargetPath is { Length: > 0 } target ? target : null;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(shell);
            }
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or InvalidCastException or Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            return null;
        }
    }
}
