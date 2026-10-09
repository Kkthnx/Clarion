namespace Clarion.Core.SystemInfo;

/// <summary>Reads the places where Windows records that an update is waiting for a restart.</summary>
public interface IPendingRestartProbe
{
    /// <summary>The Windows Update "restart required" marker. It is cleared by a restart.</summary>
    bool UpdateRebootRequired();

    /// <summary>The component servicing "restart pending" marker.</summary>
    bool ServicingRebootPending();

    /// <summary>An update installer still has work to finish at the next start.</summary>
    bool UpdateInstallerActive();
}

/// <summary>Why Windows says a restart is waiting. Empty when it does not.</summary>
public sealed record PendingRestartInfo(IReadOnlyList<string> Reasons)
{
    public bool IsPending => Reasons.Count > 0;
}

/// <summary>
/// Tells whether Windows has an update waiting for a restart. After a restart is when changes are most likely to be undone,
/// so Clarion says it will check again. The markers cannot tell a feature update from a monthly one, so nothing here claims to.
/// The list of files waiting to be deleted at restart is left out on purpose: other programs, and Clarion's own cleanup, set it all the time.
/// </summary>
public static class PendingRestart
{
    public static PendingRestartInfo Check(IPendingRestartProbe probe)
    {
        var reasons = new List<string>();
        if (Safely(probe.UpdateRebootRequired)) reasons.Add("Windows Update is waiting for a restart");
        if (Safely(probe.ServicingRebootPending)) reasons.Add("Windows servicing is waiting for a restart");
        if (Safely(probe.UpdateInstallerActive)) reasons.Add("An update installer has work left for the next start");
        return new PendingRestartInfo(reasons);
    }

    // A marker that cannot be read counts as not set. Guessing the other way would show a restart notice that is not true.
    private static bool Safely(Func<bool> read)
    {
        try { return read(); }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException) { return false; }
    }
}
