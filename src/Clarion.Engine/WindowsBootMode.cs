using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Clarion.Core.SystemInfo;

namespace Clarion.Engine;

/// <summary>Asks Windows how it was started, through the system metric made for it. Nothing is written.</summary>
[SupportedOSPlatform("windows")]
public static class WindowsBootMode
{
    private const int CleanBoot = 67; // SM_CLEANBOOT

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int index);

    public static BootMode Detect() => SafeModeNotice.FromMetric(GetSystemMetrics(CleanBoot));
}
