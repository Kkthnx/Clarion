using System.Globalization;

namespace Clarion.Core.SystemInfo;

/// <summary>What was read about Windows Update on this PC. Plain data so the judgement below can be tested.</summary>
public sealed record UpdateInputs
{
    /// <summary>The latest end of any stored pause, or null when none is stored.</summary>
    public DateTimeOffset? PauseEnds { get; init; }

    /// <summary>PausedFeatureStatus and PausedQualityStatus: 0 not paused, 1 paused, 2 resumed after a pause. Null when absent.</summary>
    public int? PausedFeatureStatus { get; init; }
    public int? PausedQualityStatus { get; init; }

    public bool WindowsUpdateServiceMissing { get; init; }
    public bool WindowsUpdateServiceDisabled { get; init; }
    public bool OrchestratorMissing { get; init; }
    public bool MedicMissing { get; init; }

    public bool AccessBlockedByPolicy { get; init; }
    public bool NoConnectPolicy { get; init; }
    public bool AutoUpdateOff { get; init; }
    public bool UseUpdateServer { get; init; }
    public string? UpdateServer { get; init; }
    public bool SettingsPageHidden { get; init; }

    /// <summary>Whether the background scan task is enabled, or null when it does not exist.</summary>
    public bool? ScanTaskEnabled { get; init; }

    public DateTimeOffset? LastCheck { get; init; }
    public DateTimeOffset? LastInstall { get; init; }
}

public enum UpdateLevel { Working, Limited, Held }

/// <summary>Good is a sign it works. Info is worth knowing and changes nothing. Limit narrows what it does. Problem stops it.</summary>
public enum UpdateFindingKind { Good, Info, Limit, Problem }

public sealed record UpdateFinding(UpdateFindingKind Kind, string Text);

public sealed record UpdateVerdict(UpdateLevel Level, IReadOnlyList<UpdateFinding> Findings)
{
    public UpdateInputs? Inputs { get; init; }

    public string Headline => Level switch
    {
        UpdateLevel.Held => "Windows Update is held back",
        UpdateLevel.Limited => "Windows Update is limited",
        _ => "Windows Update is working",
    };

    public string Explanation => Level switch
    {
        UpdateLevel.Held => "Something on this PC stops Windows from getting updates, so security fixes will not arrive until it is changed. Clarion does not change these on its own.",
        UpdateLevel.Limited => "Updates can still run, but something narrows what Windows does on its own. It is listed below.",
        _ => Findings.Any(f => f.Kind is UpdateFindingKind.Info)
            ? "Nothing found stops updates. The notes below are worth knowing, not faults."
            : "Nothing found stops updates.",
    };
}

/// <summary>
/// Reads the update settings together. A single value can mislead: a long pause can be stored while Windows reports
/// it as ended, and updates still install. So the verdict leans on what Windows says it is doing, and says when it cannot tell.
/// </summary>
public static class UpdateState
{
    /// <summary>Windows allows a pause of at most 35 days from when it is set, and one day of slack covers the time of day.</summary>
    private static readonly TimeSpan LongestPause = TimeSpan.FromDays(36);
    private static readonly TimeSpan Recent = TimeSpan.FromDays(60);

    public static UpdateVerdict Evaluate(UpdateInputs i, DateTimeOffset now)
    {
        var f = new List<UpdateFinding>();
        void Add(UpdateFindingKind kind, string text) => f.Add(new(kind, text));

        if (i.WindowsUpdateServiceMissing)
            Add(UpdateFindingKind.Problem, "The Windows Update service is not installed, so Windows cannot look for updates.");
        else if (i.WindowsUpdateServiceDisabled)
            Add(UpdateFindingKind.Problem, "The Windows Update service is turned off, so Windows cannot look for updates until it is set back to Manual.");

        if (i.OrchestratorMissing)
            Add(UpdateFindingKind.Problem, "The Update Orchestrator service is not installed, so Windows cannot schedule or start update work.");
        if (i.MedicMissing)
            Add(UpdateFindingKind.Info, "The Windows Update Medic service is not installed. It normally repairs Windows Update when something breaks it.");

        if (i.AccessBlockedByPolicy)
            Add(UpdateFindingKind.Problem, "A policy blocks access to Windows Update.");
        if (i.NoConnectPolicy)
            Add(UpdateFindingKind.Problem, "A policy stops Windows from connecting to Microsoft's update servers.");

        if (i.UseUpdateServer && !string.IsNullOrWhiteSpace(i.UpdateServer))
        {
            if (IsThisPc(i.UpdateServer))
                Add(UpdateFindingKind.Problem, $"Windows is told to get updates from {i.UpdateServer.Trim()}, a server on this PC that does not exist, so it finds nothing.");
            else
                Add(UpdateFindingKind.Limit, $"Windows is told to get updates from {i.UpdateServer.Trim()} instead of Microsoft. That is normal on a work PC and unusual on a home one.");
        }

        if (i.AutoUpdateOff)
            Add(UpdateFindingKind.Limit, "Automatic updating is turned off by a policy, so updates install only when you check by hand.");
        if (i.ScanTaskEnabled == false)
            Add(UpdateFindingKind.Limit, "The task that checks for updates in the background is disabled.");
        if (i.SettingsPageHidden)
            Add(UpdateFindingKind.Info, "The Windows Update page is hidden in Settings.");

        AddPause(i, now, Add);

        if (i.LastInstall is { } install && now - install <= Recent)
            Add(UpdateFindingKind.Good, $"Windows installed an update on {install.LocalDateTime:MMM d, yyyy}.");
        if (i.LastCheck is { } check && now - check <= TimeSpan.FromDays(14))
            Add(UpdateFindingKind.Good, $"Windows last checked for updates on {check.LocalDateTime:MMM d, yyyy}.");

        var level = f.Any(x => x.Kind == UpdateFindingKind.Problem) ? UpdateLevel.Held
            : f.Any(x => x.Kind == UpdateFindingKind.Limit) ? UpdateLevel.Limited
            : UpdateLevel.Working;
        return new UpdateVerdict(level, f) { Inputs = i };
    }

    private static void AddPause(UpdateInputs i, DateTimeOffset now, Action<UpdateFindingKind, string> add)
    {
        if (i.PauseEnds is not { } ends || ends <= now) return;

        var longer = ends - now > LongestPause;
        var ended = i.PausedFeatureStatus == 2 && i.PausedQualityStatus == 2;
        var until = ends.LocalDateTime.ToString("MMM d, yyyy", CultureInfo.CurrentCulture);
        var odd = longer ? " Windows itself only allows 35 days, so an image or a tool set this." : "";

        if (ended)
            add(UpdateFindingKind.Info, $"A pause until {until} is stored, but Windows reports it as ended.{odd}");
        else if (longer)
            add(UpdateFindingKind.Problem, $"Updates are paused until {until}.{odd}");
        else
            add(UpdateFindingKind.Limit, $"Updates are paused until {until}.");
    }

    public static bool IsThisPc(string server)
    {
        var s = server.Trim();
        if (Uri.TryCreate(s, UriKind.Absolute, out var uri)) s = uri.Host;
        return s.Equals("localhost", StringComparison.OrdinalIgnoreCase) || s is "127.0.0.1" or "::1" or "[::1]";
    }
}
