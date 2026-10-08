using Clarion.App.Controls;
using Clarion.Core.Actions;
using Clarion.Core.Model;

namespace Clarion.App.Services;

/// <summary>One repair or maintenance job as the UI shows it.</summary>
public sealed class ActionItem : IDetailSource
{
    public ActionItem(ActionDef def)
    {
        Def = def;
        Tooltip = TweakTooltip.BuildItem(
            def.Name, def.Summary, def.Recommendation, def.Advice, def.Facts, def.Benefit, def.Risk, def.RiskLevel,
            "Uses the tools that ship with Windows", BuildNotes(def));
    }

    public ActionDef Def { get; }
    public TooltipContent Tooltip { get; }

    public string Name => Def.Name;
    public string Summary => Def.Summary;
    public string Category => Def.Category;
    public string RecommendationText => Tooltip.RecommendationLabel;
    public string RiskText => DetailHelpers.RiskText(Def.RiskLevel);
    public string EvidenceText => Tooltip.EvidenceLabel;
    public string DurationText => Def.EstimatedMinutes <= 1 ? "About a minute" : $"About {Def.EstimatedMinutes} minutes";
    public IReadOnlyList<string> Provenance => [];
    public IReadOnlyList<string> ExactChanges => Def.Steps.Select(s => s.Describe()).ToList();
    public IReadOnlyList<SourceLink> SourceLinks => DetailHelpers.Links(Def.Sources);

    public ChipKind RecommendationKind => DetailHelpers.RecommendationKind(Def.Recommendation);
    public ChipKind RiskKind => DetailHelpers.RiskKind(Def.RiskLevel);

    private static IReadOnlyList<string> BuildNotes(ActionDef d)
    {
        var notes = new List<string> { $"Takes about {Math.Max(1, d.EstimatedMinutes)} minute{(d.EstimatedMinutes > 1 ? "s" : "")}." };
        if (d.RestorePoint) notes.Add("A restore point is made first.");
        if (d.NeedsReboot) notes.Add("Restart needed to finish.");
        notes.Add("You can stop it at any time. The output is shown as it runs.");
        return notes;
    }
}
