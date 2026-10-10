using Clarion.App.Controls;
using Clarion.Core.Model;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Services;

/// <summary>Shared mapping from the catalog vocabulary to chip colors and links, used by every kind of item.</summary>
public static class DetailHelpers
{
    public static ChipKind RecommendationKind(Recommendation r) => r switch
    {
        Recommendation.Recommended => ChipKind.Ok,
        Recommendation.OnlyIf => ChipKind.Warn,
        Recommendation.Avoid => ChipKind.Danger,
        _ => ChipKind.Neutral,
    };

    public static ChipKind RiskKind(RiskLevel r) => r switch
    {
        RiskLevel.Safe or RiskLevel.Low => ChipKind.Ok,
        RiskLevel.Medium => ChipKind.Warn,
        _ => ChipKind.Danger,
    };

    /// <summary>What a recommendation label means, for the label's tooltip. The details pane says why for this one setting.</summary>
    public static string RecommendationHint(Recommendation r) => r switch
    {
        Recommendation.Recommended => "Suggested for most people. The details say who it is for.",
        Recommendation.OnlyIf => "Helps in some situations and not in others. Read the details before you turn it on.",
        Recommendation.Avoid => "Not suggested for most people. Leave it alone unless you know you need it.",
        _ => "Your choice. It is safe either way, so turn it on only if you want the effect.",
    };

    /// <summary>What a risk label means, for the label's tooltip.</summary>
    public static string RiskHint(RiskLevel r) => r switch
    {
        RiskLevel.Safe => "Nothing is known to go wrong, and Revert puts it back.",
        RiskLevel.Low => "A small chance of a side effect, described in the details. Revert puts it back.",
        RiskLevel.Medium => "Can change how Windows or your programs behave. Read what could go wrong first.",
        _ => "Can break things that are hard to notice. Read what could go wrong, and make a restore point first.",
    };

    public static string RiskText(RiskLevel r) => r == RiskLevel.Safe ? "Safe" : $"{TweakTooltip.Label(r)} risk";

    public static IReadOnlyList<SourceLink> Links(IEnumerable<string> urls) => urls
        .Where(s => Uri.TryCreate(s, UriKind.Absolute, out _))
        .Select(s => { var u = new Uri(s); return new SourceLink(u.Host + u.AbsolutePath, u); })
        .ToList();

}
