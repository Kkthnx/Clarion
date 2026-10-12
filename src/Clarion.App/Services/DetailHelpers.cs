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

    /// <summary>What a recommendation label means, for the label's tooltip. The info flyout says why for this one setting.</summary>
    public static string RecommendationHint(Recommendation r) => TweakTooltip.RecommendationHint(r);

    /// <summary>What a risk label means, for the label's tooltip.</summary>
    public static string RiskHint(RiskLevel r) => TweakTooltip.RiskHint(r);

    public static string RiskText(RiskLevel r) => TweakTooltip.RiskText(r);

    public static IReadOnlyList<SourceLink> Links(IEnumerable<string> urls) => urls
        .Where(s => Uri.TryCreate(s, UriKind.Absolute, out _))
        .Select(s => { var u = new Uri(s); return new SourceLink(u.Host + u.AbsolutePath, u); })
        .ToList();

}
