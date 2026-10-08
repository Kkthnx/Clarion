using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Clarion.Engine;

/// <summary>
/// Starts a restart with a countdown through the Windows restart API with force turned off, so an app
/// with unsaved work makes Windows stop and ask. The shutdown command with a delay would force apps closed.
/// </summary>
[SupportedOSPlatform("windows")]
public static class RestartHelper
{
    private const uint ShutdownReasonPlanned = 0x80000000 | 0x00050000 | 0x00000001;

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool InitiateSystemShutdownEx(string? machine, string message, uint timeout, bool forceAppsClosed, bool reboot, uint reason);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool AbortSystemShutdown(string? machine);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool LookupPrivilegeValue(string? system, string name, out long luid);

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TokenPrivileges { public int Count; public long Luid; public int Attributes; }

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool AdjustTokenPrivileges(IntPtr token, bool disableAll, ref TokenPrivileges state, int length, IntPtr previous, IntPtr returnLength);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetCurrentProcess();

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    public static void Schedule(int seconds, string message)
    {
        EnablePrivilege("SeShutdownPrivilege");
        if (!InitiateSystemShutdownEx(null, message, (uint)seconds, forceAppsClosed: false, reboot: true, ShutdownReasonPlanned))
            throw new Win32Exception(Marshal.GetLastWin32Error());
    }

    public static void Cancel()
    {
        EnablePrivilege("SeShutdownPrivilege");
        AbortSystemShutdown(null);
    }

    private static void EnablePrivilege(string name)
    {
        const uint adjust = 0x20, query = 0x8;
        if (!OpenProcessToken(GetCurrentProcess(), adjust | query, out var token)) throw new Win32Exception(Marshal.GetLastWin32Error());
        try
        {
            if (!LookupPrivilegeValue(null, name, out var luid)) throw new Win32Exception(Marshal.GetLastWin32Error());
            var state = new TokenPrivileges { Count = 1, Luid = luid, Attributes = 2 };
            if (!AdjustTokenPrivileges(token, false, ref state, 0, IntPtr.Zero, IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        finally
        {
            CloseHandle(token);
        }
    }
}
