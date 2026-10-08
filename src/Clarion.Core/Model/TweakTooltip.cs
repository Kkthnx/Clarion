using System.Text;

namespace Clarion.Core.Model;

/// <summary>Everything a tooltip or detail card needs, so a user never has to look a setting up.</summary>
public sealed record TooltipContent(
    string Title,
    string Summary,
    string RecommendationLabel,
    Recommendation Recommendation,
    string Advice,
    IReadOnlyList<string> Facts,
    string Benefit,
    string Risk,
    string RiskLabel,
    string EvidenceLabel,
    IReadOnlyList<string> Notes)
{
    public string ToPlainText()
    {
        var sb = new StringBuilder();
        sb.AppendLine(Title);
        sb.AppendLine(Summary);
        sb.AppendLine();
        sb.AppendLine($"{RecommendationLabel}. {Advice}");
        sb.AppendLine();
        foreach (var f in Facts) sb.AppendLine($"- {f}");
        sb.AppendLine();
        sb.AppendLine($"Benefit: {Benefit}");
        sb.AppendLine($"Risk ({RiskLabel}): {Risk}");
        sb.AppendLine($"Evidence: {EvidenceLabel}");
        foreach (var n in Notes) sb.AppendLine(n);
        return sb.ToString().TrimEnd();
    }
}

public static class TweakTooltip
{
    public static string Label(Recommendation r) => r switch
    {
        Recommendation.Recommended => "Suggested: turn on",
        Recommendation.Optional => "Optional: your choice",
        Recommendation.OnlyIf => "Only if it fits you",
        Recommendation.Avoid => "Not suggested for most people",
        _ => "",
    };

    public static string Label(Evidence e) => e switch
    {
        Evidence.Proven => "Documented by the vendor",
        Evidence.Situational => "Helps in some cases",
        Evidence.Cosmetic => "Look and feel only",
        Evidence.Unproven => "Not proven by measurement",
        _ => "",
    };

    public static string Label(RiskLevel r) => r switch
    {
        RiskLevel.Safe => "Safe",
        RiskLevel.Low => "Low",
        RiskLevel.Medium => "Medium",
        RiskLevel.High => "High",
        _ => "",
    };

    public static TooltipContent Build(Tweak t)
    {
        var notes = new List<string>();
        notes.Add(t.Scope == TweakScope.Machine ? "Applies to every account on this PC. Needs administrator rights." : "Applies to your account only.");
        if (t.Requires.MinBuild >= 22000) notes.Add("Windows 11 only.");
        else if (t.Requires.MinBuild > 0) notes.Add($"Needs Windows build {t.Requires.MinBuild} or later.");
        if (t.Requires.Editions.Count > 0) notes.Add($"Works on: {string.Join(", ", t.Requires.Editions)}. Hidden on other editions.");
        if (t.NeedsReboot) notes.Add("Restart needed to finish.");
        if (t.NeedsSignOut) notes.Add("Sign out and back in to finish.");
        if (t.RestartsExplorer) notes.Add("Explorer restarts once, so the taskbar flickers.");
        notes.Add("Revert restores exactly what was there before.");

        return new TooltipContent(
            t.Name, t.Summary, Label(t.Recommendation), t.Recommendation, t.Advice,
            t.Facts, t.Benefit, t.Risk, Label(t.RiskLevel), Label(t.Evidence), notes);
    }
}
