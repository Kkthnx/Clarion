using Clarion.App.Controls;
using Clarion.Core.Updates;

namespace Clarion.App.Services;

/// <summary>One group of changes, such as "Fixed 3", ready to draw.</summary>
public sealed record ChangeGroupVm(string Title, ChipKind Kind, int Count, IReadOnlyList<string> Items);

/// <summary>One release on the timeline, ready to draw.</summary>
public sealed record ReleaseVm(
    string Version, string Summary, string BadgeText, ChipKind BadgeKind, bool ShowBadge,
    bool IsInstalled, bool IsExpanded, string? Url, IReadOnlyList<ChangeGroupVm> Groups)
{
    public bool IsNotInstalled => !IsInstalled;
    public bool HasGroups => Groups.Count > 0;
    public bool HasNoGroups => Groups.Count == 0;
    public bool HasUrl => Url is not null;
}

public static class ReleaseViews
{
    public static ChipKind ChipFor(ChangeKind kind) => kind switch
    {
        ChangeKind.New => ChipKind.Accent,
        ChangeKind.Safer => ChipKind.Ok,
        ChangeKind.Fixed => ChipKind.Warn,
        ChangeKind.Polish => ChipKind.Cyan,
        _ => ChipKind.Neutral,
    };

    public static string LabelFor(ChangeKind kind) => kind switch
    {
        ChangeKind.New => "new",
        ChangeKind.Safer => "safer",
        ChangeKind.Fixed => "fixed",
        ChangeKind.Polish => "polish",
        _ => "changes",
    };

    /// <summary>"6 new · 2 safer · 4 fixed", the same counts the pills at the top use, in the same order.</summary>
    public static string CountsText(ReleaseNotes notes) =>
        string.Join(" · ", notes.Groups.Select(g => $"{g.Items.Count} {LabelFor(g.Kind)}"));

    public static ReleaseVm From(ReleaseNotes notes, bool installed, bool expanded)
    {
        var parts = new List<string>();
        if (notes.Published is { } when) parts.Add(when.LocalDateTime.ToString("MMM d, yyyy"));
        if (notes.Groups.Count > 0) parts.Add(CountsText(notes));

        var badge = installed ? "Installed" : notes.PreRelease ? "Pre-release" : "";
        var badgeKind = installed ? ChipKind.Ok : ChipKind.Warn;

        return new ReleaseVm(
            notes.Version,
            parts.Count > 0 ? string.Join(" · ", parts) : "No notes were written for this release.",
            badge, badgeKind, badge.Length > 0, installed, expanded, notes.Url,
            notes.Groups.Select(g => new ChangeGroupVm(g.Title, ChipFor(g.Kind), g.Items.Count, g.Items)).ToList());
    }
}
