using Clarion.App.Controls;
using Clarion.Core.Model;

namespace Clarion.App.Services;

/// <summary>What the info flyout and tooltips need from any item: a setting, a cleanup row or a repair job.</summary>
public interface IDetailSource
{
    string Name { get; }
    string Summary { get; }
    string RecommendationText { get; }
    ChipKind RecommendationKind { get; }
    bool ShowRecommendation { get; }
    bool IsApp { get; }
    string? InstalledIconPath { get; }
    string? LogoFilePath { get; }
    Clarion.Core.Apps.AppIcon? Icon { get; }
    string RiskText { get; }
    ChipKind RiskKind { get; }
    string EvidenceText { get; }

    /// <summary>What a laptop needs to know about this item, or empty on a desktop and for an item with nothing to say.</summary>
    string PortableWarning { get; }
    bool HasPortableWarning { get; }
    TooltipContent Tooltip { get; }
    IReadOnlyList<string> ExactChanges { get; }
    IReadOnlyList<string> Provenance { get; }
    IReadOnlyList<SourceLink> SourceLinks { get; }
}
