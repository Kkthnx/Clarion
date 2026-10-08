using System.Text;

namespace Clarion.Core.SystemInfo;

public sealed record Fact(string Label, string Value);

public sealed record FactGroup(string Name, IReadOnlyList<Fact> Facts);

public enum InstallLevel { Standard, Possible, Likely }

/// <summary>One observation about the Windows install and how much it points to a customized image.</summary>
public sealed record InstallSignal(string Text, int Weight);

public sealed record InstallVerdict(InstallLevel Level, IReadOnlyList<InstallSignal> Signals)
{
    public string Headline => Level switch
    {
        InstallLevel.Likely => "This looks like a customized Windows image",
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
        return sb.ToString().TrimEnd();
    }
}
