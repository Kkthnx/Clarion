using System.ComponentModel;
using System.Diagnostics;

namespace Clarion.App.Services;

public enum ElevationOutcome
{
    /// <summary>A copy with administrator rights was started. This one should close.</summary>
    Started,

    /// <summary>The person said no to the Windows prompt.</summary>
    Declined,

    /// <summary>It could not be started for some other reason, which is in the log.</summary>
    Failed,
}

/// <summary>Starts Clarion again with administrator rights, using the normal Windows prompt. Clarion never elevates itself any other way.</summary>
public static class Elevation
{
    private const int CancelledByUser = 1223;

    public static ElevationOutcome RestartAsAdministrator()
    {
        var exe = Environment.ProcessPath;
        if (exe is null) return ElevationOutcome.Failed;
        try
        {
            using var started = Process.Start(new ProcessStartInfo(exe, Clarion.Core.Profiles.CliOptions.RelaunchMarker) { UseShellExecute = true, Verb = "runas", WorkingDirectory = AppContext.BaseDirectory });
            return started is null ? ElevationOutcome.Failed : ElevationOutcome.Started;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == CancelledByUser)
        {
            return ElevationOutcome.Declined;
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            Log.Write($"Could not start Clarion with administrator rights: {ex.Message}");
            return ElevationOutcome.Failed;
        }
    }
}
