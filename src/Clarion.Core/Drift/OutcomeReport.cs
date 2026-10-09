namespace Clarion.Core.Drift;

/// <summary>
/// Plain lines saying how each setting is holding up on this PC and Windows build. Optional, shown to the
/// person in full before anything is shared, and never sent by the app. Setting ids and build numbers only.
/// </summary>
public static class OutcomeReport
{
    public static IReadOnlyList<string> Lines(DriftReport report)
    {
        var lines = new List<string> { $"Windows build {report.Build}" };
        lines.AddRange(report.Holding.Order(StringComparer.Ordinal).Select(id => $"{id}: holds"));
        lines.AddRange(report.Items.OrderBy(i => i.Tweak.Id, StringComparer.Ordinal).Select(i =>
            $"{i.Tweak.Id}: {(i.IsReturnedApp ? "app came back" : i.State == Model.TweakState.Partial ? "partly changed back" : "changed back")}"));
        lines.AddRange(report.Unreadable.Order(StringComparer.Ordinal).Select(id => $"{id}: could not be read"));
        return lines;
    }
}
