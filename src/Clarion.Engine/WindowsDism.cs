using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Clarion.Core.Features;

namespace Clarion.Engine;

/// <summary>
/// Reads the state of Windows features and capabilities through the DISM API that Windows ships (dismapi.dll), in this process, instead of
/// starting PowerShell. On the PC this was built on, seven features took 0.45 seconds through the API and 1.85 seconds through
/// Get-WindowsOptionalFeature, which has to start PowerShell and load its module first. Both give the same states.
/// Every method answers null when the API cannot be used, for example without administrator rights, and the caller then asks PowerShell
/// as before, so nothing depends on the API being there. Nothing is written.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class WindowsDism
{
    // The name Microsoft's DISM API reference gives to the Windows that is running, DISM_ONLINE_IMAGE.
    private const string OnlineImage = "DISM_{53BFAE52-B167-4E2F-A258-0A37B57FF845}";

    // CBS_E_UNKNOWN_FEATURE: this PC has no feature of that name, which PowerShell also reports as nothing.
    private const int UnknownFeature = unchecked((int)0x800F080C);

    // The API keeps one set of state per process, so only one reader runs at a time.
    private static readonly object Gate = new();

    [DllImport("dismapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DismInitialize(int logLevel, string? logFilePath, string? scratchDirectory);

    [DllImport("dismapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DismOpenSession(string imagePath, string? windowsDirectory, string? systemDrive, out uint session);

    [DllImport("dismapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DismGetFeatureInfo(uint session, string featureName, string? identifier, int packageIdentifier, out IntPtr info);

    [DllImport("dismapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DismGetCapabilityInfo(uint session, string name, out IntPtr info);

    [DllImport("dismapi.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DismDelete(IntPtr structure);

    [DllImport("dismapi.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DismCloseSession(uint session);

    [DllImport("dismapi.dll", ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int DismShutdown();

    /// <summary>The state names of the features this PC has. A name it does not have is left out. Null when the API could not be used.</summary>
    public static Dictionary<string, string>? ReadFeatures(IReadOnlyCollection<string> names) =>
        WithSession<Dictionary<string, string>>(session =>
        {
            var found = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var name in names)
            {
                var hr = DismGetFeatureInfo(session, name, null, 0, out var info);
                if (hr == UnknownFeature) continue;
                if (hr != 0) return null;
                found[name] = FeatureRules.FeatureStateName(StateOf(info));
            }
            return found;
        });

    /// <summary>The state name of one capability. Like PowerShell, a capability this PC does not know is NotPresent. Null when the API could not be used.</summary>
    public static string? ReadCapability(string name) =>
        WithSession<string>(session =>
        {
            var hr = DismGetCapabilityInfo(session, name, out var info);
            return hr == 0 ? FeatureRules.CapabilityStateName(StateOf(info)) : null;
        });

    /// <summary>
    /// Both DismFeatureInfo and DismCapabilityInfo start with a pointer to the name, followed by the state as a 32 bit number. The pointer
    /// is eight bytes on the 64 bit program this is, and the structure is released with DismDelete after the number is read.
    /// </summary>
    private static int StateOf(IntPtr info)
    {
        try { return Marshal.ReadInt32(info, IntPtr.Size); }
        finally { DismDelete(info); }
    }

    private static T? WithSession<T>(Func<uint, T?> read) where T : class
    {
        lock (Gate)
        {
            var started = false;
            uint session = 0;
            var opened = false;
            try
            {
                if (DismInitialize(0, null, null) != 0) return null;
                started = true;
                if (DismOpenSession(OnlineImage, null, null, out session) != 0) return null;
                opened = true;
                return read(session);
            }
            catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException or SEHException)
            {
                return null;
            }
            finally
            {
                if (opened) DismCloseSession(session);
                if (started) DismShutdown();
            }
        }
    }
}
