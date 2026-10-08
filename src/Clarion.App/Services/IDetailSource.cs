using Clarion.App.Controls;
using Clarion.Core.Model;

namespace Clarion.App.Services;

/// <summary>What the detail pane and tooltips need from any item: a setting, a cleanup row or a repair job.</summary>
public interface IDetailSource
{
    string Name { get; }
    string Summary { get; }
    string RecommendationText { get; }
    ChipKind RecommendationKind { get; }
    string RiskText { get; }
    ChipKind RiskKind { get; }
    string EvidenceText { get; }
    TooltipContent Tooltip { get; }
    IReadOnlyList<string> ExactChanges { get; }
    IReadOnlyList<string> Provenance { get; }
    IReadOnlyList<SourceLink> SourceLinks { get; }
}
