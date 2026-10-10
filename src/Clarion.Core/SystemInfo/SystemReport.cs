using System.Text;

namespace Clarion.Core.SystemInfo;

public sealed record Fact(string Label, string Value);

public sealed record FactGroup(string Name, IReadOnlyList<Fact> Facts);

public enum InstallLevel { Standard, Possible, Likely }

/// <summary>One observation about the Windows install and how much it points to a customized image.</summary>
public sealed record InstallSignal(string Text, int Weight);

public sealed record InstallVerdict(InstallLevel Level, IReadOnlyList<InstallSignal> Signals)
{
    /// <summary>What was observed on this PC, kept so settings can be matched against it.</summary>
    public InstallInputs? Inputs { get; init; }

    /// <summary>The image Clarion thinks this is, such as Ghost Spectre, or null.</summary>
    public string? ImageName { get; init; }

    /// <summary>True when the name came from the system information or a marker, false when it is a guess from a pattern of settings.</summary>
    public bool ImageNameIsFirm { get; init; } = true;

    /// <summary>The name as it reads in a sentence, with "possibly" when it is a guess. Empty when there is none.</summary>
    public string ImageLabel => ImageName is null ? "" : ImageNameIsFirm ? ImageName : $"possibly {ImageName}";

    public string Headline => Level switch
    {
        InstallLevel.Likely when ImageName is not null => ImageNameIsFirm
            ? $"This looks like {ImageName}, a customized Windows image"
            : $"This looks like a customized Windows image, {ImageLabel}",
        InstallLevel.Likely => "This looks like a customized Windows image",
        InstallLevel.Possible when ImageName is not null => $"This Windows install may be a customized image, {ImageLabel}",
        InstallLevel.Possible => "This Windows install may have been customized",
        _ => "This looks like a standard Windows install",
    };

    public string Explanation => Level switch
    {
        InstallLevel.Likely => "Parts of Windows were probably removed or changed before it was installed. Some settings will show as not available, and Windows Update may behave differently. Clarion only changes what you choose.",
        InstallLevel.Possible => "A few things differ from a standard install. That is common after manual cleanup too, so this is only a hint.",
        _ => "Nothing unusual was found. This is a check of known signs, not proof.",
    };
}

public sealed record SystemReport(IReadOnlyList<FactGroup> Groups, InstallVerdict Install)
{
    /// <summary>How Windows Update looks on this PC, or null when it could not be read.</summary>
    public UpdateVerdict? Updates { get; init; }

    /// <summary>Plain text for pasting into a bug report or forum post.</summary>
    public string ToText()
    {
        var sb = new StringBuilder();
        foreach (var g in Groups)
        {
            sb.AppendLine(g.Name);
            foreach (var f in g.Facts) sb.AppendLine($"  {f.Label}: {f.Value}");
            sb.AppendLine();
        }
        sb.AppendLine("Windows install check");
        sb.AppendLine($"  {Install.Headline}");
        foreach (var s in Install.Signals) sb.AppendLine($"  - {s.Text}");
        if (Updates is not null)
        {
            sb.AppendLine();
            sb.AppendLine("Windows Update");
            sb.AppendLine($"  {Updates.Headline}");
            foreach (var u in Updates.Findings) sb.AppendLine($"  - {u.Text}");
        }
        return sb.ToString().TrimEnd();
    }
}
