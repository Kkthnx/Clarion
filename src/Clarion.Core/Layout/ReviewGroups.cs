namespace Clarion.Core.Layout;

/// <summary>What a queued change does, which is how the review sorts a long queue so it can be read in parts.</summary>
public enum ReviewKind
{
    TurnOn,
    RemoveWindowsApps,
    InstallApps,
    UninstallApps,
    Revert,
}

/// <summary>
/// Sorting a long review into sections. A queue of three changes reads fine as one list. A queue of thirty mixes registry settings, apps being
/// installed and apps being removed, and the person is asked to accept all of it at once, so each kind gets its own heading and count and can be
/// folded away once read. Nothing is folded at first, because a section the person has not looked at is not one they have agreed to.
/// </summary>
public static class ReviewGroups
{
    /// <summary>A queue shorter than this stays one plain list, whatever it holds.</summary>
    public const int GroupFrom = 6;

    /// <summary>The order the sections appear in: settings first, then what is added, then what is taken away, then reverts.</summary>
    public static readonly IReadOnlyList<ReviewKind> Order =
        [ReviewKind.TurnOn, ReviewKind.InstallApps, ReviewKind.RemoveWindowsApps, ReviewKind.UninstallApps, ReviewKind.Revert];

    public static ReviewKind Of(bool installsApps, bool removesWindowsApps, bool turnsOn) =>
        installsApps ? (turnsOn ? ReviewKind.InstallApps : ReviewKind.UninstallApps)
        : turnsOn ? (removesWindowsApps ? ReviewKind.RemoveWindowsApps : ReviewKind.TurnOn)
        : ReviewKind.Revert;

    /// <summary>Sections are used when the queue is long enough to need them and holds more than one kind of change.</summary>
    public static bool ShouldGroup(IReadOnlyCollection<ReviewKind> kinds) =>
        kinds.Count >= GroupFrom && kinds.Distinct().Count() > 1;

    public static string Title(ReviewKind kind, int count) => kind switch
    {
        ReviewKind.TurnOn => $"Settings to turn on ({count})",
        ReviewKind.InstallApps => $"Apps to install ({count})",
        ReviewKind.RemoveWindowsApps => $"Windows apps to remove ({count})",
        ReviewKind.UninstallApps => $"Apps to uninstall ({count})",
        _ => $"Settings to revert ({count})",
    };
}
