using System.Text;
using Clarion.Core.Journal;

namespace Clarion.Core.Reports;

/// <summary>
/// The list of what .Clarion has changed on this PC that is still on, as plain text a person can keep, print or send to someone who is helping
/// them. Each setting shows when it was applied and what each step set, and under it the step that puts the old value back, which is the same
/// record Revert uses. The caller removes the computer name, user name and network addresses before it leaves the app.
/// </summary>
public static class ChangeReport
{
    public static string Build(
        IReadOnlyDictionary<string, IReadOnlyList<JournalEntry>> outstanding,
        IReadOnlyDictionary<string, string> names,
        DateTimeOffset now,
        string version)
    {
        var sb = new StringBuilder();
        sb.AppendLine($".Clarion change report, made {now.LocalDateTime:yyyy-MM-dd HH:mm} by version {version}");
        sb.AppendLine();
        if (outstanding.Count == 0)
        {
            sb.AppendLine(".Clarion has not changed anything that is still on.");
            return sb.ToString();
        }

        sb.AppendLine($"{outstanding.Count} setting{(outstanding.Count == 1 ? "" : "s")} changed by .Clarion and still on, newest first.");
        sb.AppendLine("Each shows what was set, and under it what Revert puts back.");

        foreach (var (id, entries) in outstanding.OrderByDescending(kv => kv.Value[0].Time))
        {
            sb.AppendLine();
            var name = names.TryGetValue(id, out var n) ? n : id;
            sb.AppendLine($"{name} ({id})");
            sb.AppendLine($"  Applied {entries[0].Time.LocalDateTime:yyyy-MM-dd HH:mm}");
            foreach (var entry in entries)
            {
                sb.AppendLine($"  Set:      {entry.Operation.Describe()}");
                sb.AppendLine($"  Revert:   {entry.Undo.Describe()}");
            }
        }
        return sb.ToString();
    }
}
