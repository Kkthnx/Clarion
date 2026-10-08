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

    public static string RiskText(RiskLevel r) => r == RiskLevel.Safe ? "Safe" : $"{TweakTooltip.Label(r)} risk";

    public static IReadOnlyList<SourceLink> Links(IEnumerable<string> urls) => urls
        .Where(s => Uri.TryCreate(s, UriKind.Absolute, out _))
        .Select(s => { var u = new Uri(s); return new SourceLink(u.Host + u.AbsolutePath, u); })
        .ToList();

    /// <summary>
    /// Builds a tooltip body the first time it opens, so long lists do not build hundreds of controls up front.
    /// The tooltip's Tag must hold the item.
    /// </summary>
    public static void FillTipOnOpen(object sender)
    {
        if (sender is not ToolTip tip || tip.Content is TweakDetail) return;
        if (tip.Tag is not IDetailSource item) return;
        tip.Content = new TweakDetail { Item = item, ShowTechnical = false, Width = 400 };
    }
}
