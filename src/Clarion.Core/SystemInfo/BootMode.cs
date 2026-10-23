namespace Clarion.Core.SystemInfo;

/// <summary>How Windows was started. Safe Mode runs without most of its services.</summary>
public enum BootMode { Normal, SafeMode, SafeModeWithNetwork }

public static class SafeModeNotice
{
    public const string Title = "Windows is in Safe Mode";

    /// <summary>
    /// What stops working and what does not. Safe Mode starts only the services Windows needs, so reading or changing an app or a scheduled
    /// task fails there (the Windows app list answers that its service cannot be started in Safe Mode), while settings kept in the registry
    /// are read and written as usual. A restore point is not promised, because it was not tried there.
    /// </summary>
    public static string Message(BootMode mode) => mode == BootMode.Normal ? "" :
        "Windows apps, scheduled tasks and Windows features cannot be read here, so they show as not available. " +
        "Settings kept in the registry still work, but a restore point may not be possible. " +
        "Restart Windows normally to see and change everything.";

    /// <summary>The value of the system metric that says how Windows was started: 0 normal, 1 Safe Mode, 2 Safe Mode with networking.</summary>
    public static BootMode FromMetric(int value) => value switch
    {
        0 => BootMode.Normal,
        2 => BootMode.SafeModeWithNetwork,
        _ => BootMode.SafeMode,
    };
}
