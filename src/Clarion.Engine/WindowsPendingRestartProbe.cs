using System.Runtime.Versioning;
using Clarion.Core.SystemInfo;
using Microsoft.Win32;

namespace Clarion.Engine;

/// <summary>Reads Windows' own restart markers from the registry. Nothing is written.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsPendingRestartProbe : IPendingRestartProbe
{
    public bool UpdateRebootRequired() =>
        KeyExists("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate\\Auto Update\\RebootRequired");

    public bool ServicingRebootPending() =>
        KeyExists("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Component Based Servicing\\RebootPending");

    public bool UpdateInstallerActive()
    {
        using var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Updates");
        return key?.GetValue("UpdateExeVolatile") is int value && value != 0;
    }

    private static bool KeyExists(string path)
    {
        using var key = Registry.LocalMachine.OpenSubKey(path);
        return key is not null;
    }
}
