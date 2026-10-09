using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Clarion.Core.Cleanup;
using Microsoft.Win32;

namespace Clarion.Engine;

/// <summary>The real Windows side of cleanup: folders, launchers, processes, the restart queue and the bin.</summary>
[SupportedOSPlatform("windows")]
public sealed partial class WindowsCleanupPlatform : ICleanupPlatform
{
    private static readonly Guid LocalAppDataLow = new("A520A1A4-1780-4FF6-BD18-167343C5AF16");

    [DllImport("shell32.dll")]
    private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid id, uint flags, IntPtr token, out IntPtr path);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool MoveFileEx(string existing, string? newName, int flags);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ShQueryRbInfo { public uint Size; public long Bytes; public long Items; }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHQueryRecycleBin(string? root, ref ShQueryRbInfo info);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? root, uint flags);

    private const int MoveDelayUntilReboot = 0x4;
    private const uint NoConfirmNoProgressNoSound = 0x7;

    private readonly string _localLow = KnownLocalLow();

    public string Expand(string template) =>
        Environment.ExpandEnvironmentVariables(template.Replace("%LOCALLOW%", _localLow, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<string> AllowedBases =>
    [
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        _localLow,
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
    ];

    public IReadOnlyList<string> AllowedExact
    {
        get
        {
            var drive = Environment.GetEnvironmentVariable("SystemDrive") ?? "C:";
            return [drive + "\\AMD", drive + "\\NVIDIA\\DisplayDriver"];
        }
    }

    public IReadOnlyList<string> SteamLibraries()
    {
        var found = new List<string>();
        var steam = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", null) as string;
        if (string.IsNullOrWhiteSpace(steam)) return found;

        steam = steam.Replace('/', '\\');
        if (Directory.Exists(Path.Combine(steam, "steamapps"))) found.Add(steam);

        var vdf = Path.Combine(steam, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdf)) return found;
        try
        {
            foreach (Match m in PathLine().Matches(File.ReadAllText(vdf)))
            {
                var lib = m.Groups[1].Value.Replace("\\\\", "\\");
                if (Directory.Exists(Path.Combine(lib, "steamapps")) && !found.Contains(lib, StringComparer.OrdinalIgnoreCase)) found.Add(lib);
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
        return found;
    }

    [GeneratedRegex("\"path\"\\s+\"([^\"]+)\"", RegexOptions.IgnoreCase)]
    private static partial Regex PathLine();

    public IReadOnlyList<string> WowVersionFolders()
    {
        var found = new List<string>();
        foreach (var view in new[] { RegistryView.Registry32, RegistryView.Registry64 })
        {
            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            using var key = hklm.OpenSubKey("SOFTWARE\\Blizzard Entertainment\\World of Warcraft");
            if (key is null) continue;

            AddIfWow(key.GetValue("InstallPath") as string);
            foreach (var sub in key.GetSubKeyNames())
            {
                using var child = key.OpenSubKey(sub);
                AddIfWow(child?.GetValue("InstallPath") as string);
            }
        }

        // Sibling versions in the same install, such as _classic_ or _ptr_.
        foreach (var folder in found.ToList())
        {
            var parent = Path.GetDirectoryName(folder.TrimEnd('\\'));
            if (parent is null || !Directory.Exists(parent)) continue;
            foreach (var sibling in Directory.EnumerateDirectories(parent, "_*")) AddIfWow(sibling);
        }
        return found;

        void AddIfWow(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            var full = Path.GetFullPath(path).TrimEnd('\\');
            if (Directory.Exists(full) && Directory.EnumerateFiles(full, "Wow*.exe").Any() && !found.Contains(full, StringComparer.OrdinalIgnoreCase))
                found.Add(full);
        }
    }

    public IReadOnlySet<string> RunningProcesses()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in Process.GetProcesses())
        {
            names.Add(p.ProcessName);
            p.Dispose();
        }
        return names;
    }

    public bool QueueDeleteAtRestart(string file) => MoveFileEx(file, null, MoveDelayUntilReboot);

    public string DataFolder => EngineFactory.DefaultDataDirectory;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFile(string name, uint access, uint share, IntPtr security, uint disposition, uint flags, IntPtr template);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    private const uint DeleteAccess = 0x00010000;
    private const uint ShareAll = 0x7;
    private const uint OpenExisting = 3;
    private const int SharingViolation = 32;
    private const int AccessDenied = 5;

    /// <summary>Asks for delete access while allowing others to read, write and delete. That is refused only when another program holds the file open without allowing delete.</summary>
    public bool IsLocked(string file)
    {
        var handle = CreateFile(file, DeleteAccess, ShareAll, IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
        if (handle != new IntPtr(-1))
        {
            CloseHandle(handle);
            return false;
        }
        var error = Marshal.GetLastWin32Error();
        return error is SharingViolation or AccessDenied;
    }

    public long RecycleBinBytes()
    {
        var info = new ShQueryRbInfo { Size = (uint)Marshal.SizeOf<ShQueryRbInfo>() };
        return SHQueryRecycleBin(null, ref info) == 0 ? info.Bytes : 0;
    }

    public void EmptyRecycleBin()
    {
        var hr = SHEmptyRecycleBin(IntPtr.Zero, null, NoConfirmNoProgressNoSound);
        // 0x8000FFFF means the bin was already empty on some versions.
        if (hr != 0 && hr != unchecked((int)0x8000FFFF)) throw new InvalidOperationException($"Windows could not empty the Recycle Bin (0x{hr:X8}).");
    }

    /// <summary>The classic logs such as Application and System. The thousands of component logs are left alone.</summary>
    public IReadOnlyList<string> EventLogNames()
    {
        using var session = new EventLogSession();
        return session.GetLogNames().Where(n => !n.Contains('/')).Order().ToList();
    }

    public void ClearEventLog(string name, string backupFile)
    {
        if (name.Equals("Security", StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("The Security log is never cleared.");
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(backupFile)!);
            using var session = new EventLogSession();
            // Windows writes the copy first, and does not clear the log if the copy fails.
            session.ClearLog(name, backupFile);
        }
        catch (EventLogException ex)
        {
            throw new InvalidOperationException($"{name}: {ex.Message}", ex);
        }
    }

    private static string KnownLocalLow()
    {
        if (SHGetKnownFolderPath(LocalAppDataLow, 0, IntPtr.Zero, out var ptr) == 0)
        {
            try { return Marshal.PtrToStringUni(ptr) ?? Fallback(); }
            finally { Marshal.FreeCoTaskMem(ptr); }
        }
        return Fallback();

        static string Fallback() => Path.Combine(
            Path.GetDirectoryName(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))!, "LocalLow");
    }
}
