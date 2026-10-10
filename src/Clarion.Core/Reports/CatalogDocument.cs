using System.Text;
using Clarion.Core.Model;

namespace Clarion.Core.Reports;

/// <summary>
/// The settings reference, written from the catalog itself so it cannot disagree with what the app does. It is printed by
/// <c>Clarion.exe --catalog-doc</c> and kept in <c>docs/CATALOG.md</c>, and a test fails when that file is out of date.
/// </summary>
public static class CatalogDocument
{
    public static string Markdown(IEnumerable<Tweak> tweaks)
    {
        var all = tweaks.ToList();
        var sb = new StringBuilder();
        sb.Append("# Settings reference\n\n");
        sb.Append("This page is written from the catalog by `Clarion.exe --catalog-doc`. Do not edit it by hand. Change the catalog and write the page again.\n\n");
        sb.Append($"{all.Count} settings. Each one names what it changes, how risky it is, how well the effect is documented, and where that comes from.\n");

        foreach (var category in all.GroupBy(t => t.Category).OrderBy(g => g.Key, StringComparer.Ordinal))
        {
            sb.Append($"\n## {category.Key}\n");
            foreach (var topic in category.GroupBy(t => t.Topic).OrderBy(g => g.Key, StringComparer.Ordinal))
            {
                if (topic.Key.Length > 0) sb.Append($"\n### {topic.Key}\n");
                foreach (var t in topic.OrderBy(t => t.Name, StringComparer.Ordinal).ThenBy(t => t.Id, StringComparer.Ordinal)) Entry(sb, t, topic.Key.Length > 0 ? 4 : 3);
            }
        }
        return sb.ToString();
    }

    private static void Entry(StringBuilder sb, Tweak t, int level)
    {
        var tip = TweakTooltip.Build(t);
        sb.Append($"\n{new string('#', level)} {t.Name}\n\n");
        var checkedOn = t.LastVerifiedBuild > 0 ? $"checked on build {t.LastVerifiedBuild}" : "not checked on a build yet";
        sb.Append($"`{t.Id}` · {tip.RiskLabel} · {tip.EvidenceLabel} · {tip.RecommendationLabel} · {checkedOn}\n\n");
        sb.Append($"{t.Summary}\n\n");
        sb.Append($"- **What it does:** {t.What}\n");
        sb.Append($"- **Benefit:** {t.Benefit}\n");
        sb.Append($"- **Risk:** {t.Risk}\n");
        sb.Append("- **Changes:**\n");
        foreach (var op in t.Apply) sb.Append($"  - `{op.Describe()}`\n");

        var needs = new List<string>();
        if (t.Requires.MinBuild > 0) needs.Add($"Windows build {t.Requires.MinBuild} or later");
        if (t.Requires.Editions.Count > 0) needs.Add(string.Join(", ", t.Requires.Editions));
        if (t.Scope == TweakScope.Machine) needs.Add("administrator rights, applies to every account");
        if (t.NeedsReboot) needs.Add("a restart");
        if (t.NeedsSignOut) needs.Add("signing out");
        if (t.RestartsExplorer) needs.Add("File Explorer restarts");
        if (needs.Count > 0) sb.Append($"- **Needs:** {string.Join("; ", needs)}\n");

        if (t.Sources.Count > 0) sb.Append($"- **Sources:** {string.Join(", ", t.Sources.Select(s => $"<{s}>"))}\n");
    }
}
