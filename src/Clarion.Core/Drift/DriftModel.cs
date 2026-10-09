using Clarion.Core.Model;

namespace Clarion.Core.Drift;

/// <summary>A setting Clarion applied that no longer holds.</summary>
/// <param name="State">NotApplied when every step came undone, Partial when only some did.</param>
/// <param name="Changed">The steps that no longer hold, in plain words.</param>
/// <param name="ReturnedApps">Package names of removed apps that are installed again.</param>
public sealed record DriftItem(
    Tweak Tweak,
    TweakState State,
    DateTimeOffset AppliedAt,
    IReadOnlyList<string> Changed,
    IReadOnlyList<string> ReturnedApps)
{
    public bool IsReturnedApp => ReturnedApps.Count > 0;
}

/// <summary>The result of one verify scan.</summary>
/// <param name="PreviousBuild">The Windows build at the last scan, or null on the first one.</param>
/// <param name="Checked">How many settings Clarion applied and could read.</param>
/// <param name="Unreadable">Ids of settings that could not be read this time.</param>
public sealed record DriftReport(
    DateTimeOffset ScannedAt,
    int Build,
    int? PreviousBuild,
    int Checked,
    IReadOnlyList<DriftItem> Items,
    IReadOnlyList<string> Unreadable)
{
    public IReadOnlyList<DriftItem> ReturnedApps => Items.Where(i => i.IsReturnedApp).ToList();
    public IReadOnlyList<DriftItem> ChangedBack => Items.Where(i => !i.IsReturnedApp).ToList();
    public bool HasDrift => Items.Count > 0;

    /// <summary>True when Windows moved to a different build since the last scan, which is when most of these come back.</summary>
    public bool FeatureUpdateSinceLastScan => PreviousBuild is { } p && p != Build;
}
