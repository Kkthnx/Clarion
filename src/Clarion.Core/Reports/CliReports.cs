using System.Text;
using Clarion.Core.Drift;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.SystemInfo;
using Clarion.Core.Troubleshoot;

namespace Clarion.Core.Reports;

/// <summary>The plain text the command line prints for the read-only commands, kept here so it can be tested.</summary>
public static class CliReports
{
    /// <summary>
    /// Exit code for --verify: 0 when everything holds, 1 when something needs a look. When the result is being saved (the monthly check),
    /// finding changes is the job and not a failure, so it is 0. Task Scheduler would otherwise list every useful run as an error.
    /// </summary>
    public static int VerifyExitCode(DriftReport report, bool resultSaved = false) =>
        resultSaved ? 0 : report.HasDrift || report.Unreadable.Count > 0 ? 1 : 0;

    public static string Verify(DriftReport report, PendingRestartInfo? pending = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Checked {report.Checked} setting{(report.Checked == 1 ? "" : "s")} Clarion applied, on Windows build {report.Build}.");
        if (pending is { IsPending: true })
            sb.AppendLine($"{string.Join(". ", pending.Reasons)}. Clarion will need to check again after the restart.");
        if (report.FeatureUpdateSinceLastScan)
            sb.AppendLine($"Windows went from build {report.PreviousBuild} to {report.Build} since the last check.");

        foreach (var item in report.ReturnedApps)
        {
            sb.AppendLine();
            sb.AppendLine($"APP CAME BACK  {item.Tweak.Name}  ({item.Tweak.Id})");
            foreach (var line in item.Changed) sb.AppendLine($"  {line}");
        }
        foreach (var item in report.ChangedBack)
        {
            sb.AppendLine();
            sb.AppendLine($"CHANGED BACK  {item.Tweak.Name}  ({item.Tweak.Id})");
            foreach (var line in item.Changed) sb.AppendLine($"  {line}");
        }
        if (report.Unreadable.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"Could not be read: {string.Join(", ", report.Unreadable)}. Some checks need administrator rights.");
        }

        sb.AppendLine();
        sb.AppendLine(report.HasDrift
            ? $"{report.Items.Count} setting{(report.Items.Count == 1 ? "" : "s")} no longer in effect. Open Clarion and go to Verify to put them back."
            : report.Unreadable.Count > 0 ? "Nothing was found that changed back, but some settings could not be checked."
            : "Everything Clarion applied is still in place.");
        return sb.ToString().TrimEnd();
    }

    public static string Symptoms(IEnumerable<Symptom> symptoms) =>
        string.Join(Environment.NewLine, symptoms.Select(s => $"{s.Id,-22} {s.Title}"));

    public static string WhatBroke(Symptom symptom, IReadOnlyList<Suspect> suspects, DateTimeOffset? since = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine(symptom.Title);
        sb.AppendLine(symptom.Tip);
        if (since is not null) sb.AppendLine($"Only changes made on or after {since.Value:yyyy-MM-dd} are listed.");
        sb.AppendLine();
        if (suspects.Count == 0)
        {
            sb.AppendLine("Clarion did not change anything that could explain this. Other usual causes: a customized Windows image, another cleanup tool, a driver or Windows update.");
            return sb.ToString().TrimEnd();
        }
        foreach (var s in suspects)
            sb.AppendLine($"{s.AppliedAt.LocalDateTime:yyyy-MM-dd HH:mm}  {(s.Direct ? "related " : "same area")}  {s.Tweak.Name}  ({s.Tweak.Id})");
        return sb.ToString().TrimEnd();
    }

    public static string History(IReadOnlyDictionary<string, IReadOnlyList<JournalEntry>> outstanding, IEnumerable<Tweak> catalog)
    {
        var names = catalog.ToDictionary(t => t.Id, t => t.Name, StringComparer.OrdinalIgnoreCase);
        var rows = outstanding
            .Select(kv => (Id: kv.Key, When: kv.Value[0].Time, Name: names.TryGetValue(kv.Key, out var n) ? n : kv.Key))
            .OrderByDescending(r => r.When)
            .Select(r => $"{r.When.LocalDateTime:yyyy-MM-dd HH:mm}  {r.Name}  ({r.Id})")
            .ToList();
        return rows.Count == 0 ? "Clarion has not changed anything that is still on." : string.Join(Environment.NewLine, rows);
    }
}
