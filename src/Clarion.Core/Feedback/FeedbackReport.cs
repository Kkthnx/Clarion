using System.Text;
using Clarion.Core.SystemInfo;

namespace Clarion.Core.Feedback;

public enum FeedbackKind { Bug, Suggestion }

/// <summary>What the person typed and what they chose to include.</summary>
public sealed record FeedbackInput(
    FeedbackKind Kind,
    string Title,
    string Description,
    string AppVersion,
    bool IncludeSystem,
    bool IncludeLog,
    SystemReport? System,
    IReadOnlyList<string> AppliedSettings,
    IReadOnlyList<string> LogLines,
    IReadOnlyList<string>? Outcomes = null);

/// <summary>Builds the text of a bug report or suggestion and the address that opens it on GitHub.</summary>
public static class FeedbackReport
{
    public const string Repository = "Kkthnx/Clarion";

    // Browsers and GitHub handle long addresses poorly, so past this the full text goes to the clipboard instead.
    public const int MaxUrlLength = 7000;

    // Facts that identify the PC or person are never copied, even when they are part of the system report.
    private static readonly string[] NeverShared = ["Last restart", "Installed"];

    public static string Build(FeedbackInput input, string userName, string machineName)
    {
        var sb = new StringBuilder();
        var description = input.Description.Trim();
        sb.AppendLine(input.Kind == FeedbackKind.Bug ? "### What happened" : "### Suggestion");
        sb.AppendLine(description.Length > 0 ? description : "_No description given._");
        sb.AppendLine();
        sb.AppendLine("### Clarion");
        sb.AppendLine($"- Version: {input.AppVersion}");

        if (input.IncludeSystem && input.System is { } report)
        {
            sb.AppendLine();
            sb.AppendLine("### This PC");
            foreach (var g in report.Groups)
                foreach (var f in g.Facts.Where(f => !NeverShared.Contains(f.Label, StringComparer.OrdinalIgnoreCase)))
                    sb.AppendLine($"- {f.Label}: {f.Value}");
            sb.AppendLine($"- Windows install check: {report.Install.Headline}");
            foreach (var s in report.Install.Signals) sb.AppendLine($"  - {s.Text}");
        }

        if (input.AppliedSettings.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("### Settings Clarion has applied");
            sb.AppendLine(string.Join(", ", input.AppliedSettings.Order(StringComparer.Ordinal)));
        }

        if (input.Outcomes is { Count: > 0 })
        {
            sb.AppendLine();
            sb.AppendLine("### How settings are holding up");
            foreach (var line in input.Outcomes) sb.AppendLine($"- {line}");
        }

        if (input.IncludeLog && input.LogLines.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("### Recent activity");
            sb.AppendLine("```");
            foreach (var line in input.LogLines) sb.AppendLine(line);
            sb.AppendLine("```");
        }

        // The person's own words are cleaned too, since a pasted path or address is easy to miss.
        return Sanitizer.Clean(sb.ToString().TrimEnd(), userName, machineName);
    }

    public static string CleanTitle(FeedbackInput input, string userName, string machineName)
    {
        var title = Sanitizer.Clean(input.Title.Trim(), userName, machineName);
        if (title.Length == 0) title = input.Kind == FeedbackKind.Bug ? "Problem report" : "Suggestion";
        return (input.Kind == FeedbackKind.Bug ? "Bug: " : "Suggestion: ") + title;
    }

    /// <summary>
    /// The address that opens a new issue with the title and text filled in. When the text is too long for an
    /// address, the page is opened with a short note and the caller must put the full text on the clipboard.
    /// </summary>
    public static (Uri Url, bool NeedsPaste) IssueUrl(FeedbackKind kind, string title, string body)
    {
        string Make(string b) =>
            $"https://github.com/{Repository}/issues/new?labels={(kind == FeedbackKind.Bug ? "bug" : "enhancement")}" +
            $"&title={Uri.EscapeDataString(title)}&body={Uri.EscapeDataString(b)}";

        var full = Make(body);
        if (full.Length <= MaxUrlLength) return (new Uri(full), false);
        return (new Uri(Make("The report was copied to your clipboard. Paste it here with Ctrl+V.")), true);
    }
}
