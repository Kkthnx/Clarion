using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace Clarion.Engine;

public sealed record AccountInfo(string ElevatedAs, string SignedInAs, bool Differs);

/// <summary>
/// Detects when Clarion was elevated with a different administrator account than the one signed in.
/// Per account settings then change that other account, not the one you use every day.
/// </summary>
[SupportedOSPlatform("windows")]
public static class AccountCheck
{
    private const uint ProcessQueryLimitedInformation = 0x1000;
    private const uint TokenQuery = 0x0008;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern SafeProcessHandle OpenProcess(uint access, bool inherit, int pid);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(SafeProcessHandle process, uint access, out IntPtr token);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    public static AccountInfo Detect()
    {
        using var me = WindowsIdentity.GetCurrent();
        var meName = me.Name ?? "";

        foreach (var p in System.Diagnostics.Process.GetProcessesByName("explorer"))
        {
            try
            {
                using var handle = OpenProcess(ProcessQueryLimitedInformation, false, p.Id);
                if (handle.IsInvalid || !OpenProcessToken(handle, TokenQuery, out var token)) continue;
                try
                {
                    using var shell = new WindowsIdentity(token);
                    var differs = shell.User is not null && me.User is not null && shell.User != me.User;
                    return new AccountInfo(meName, shell.Name ?? "", differs);
                }
                finally
                {
                    CloseHandle(token);
                }
            }
            finally
            {
                p.Dispose();
            }
        }
        return new AccountInfo(meName, meName, false);
    }
}
