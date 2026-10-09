using System.Reflection;

namespace Clarion.Core.Updates;

/// <summary>The kinds of change the changelog groups things under, in the order they are shown.</summary>
public enum ChangeKind { New, Safer, Fixed, Polish, Other }

public sealed record ChangeGroup(ChangeKind Kind, IReadOnlyList<string> Items)
{
    public string Title => Kind switch
    {
        ChangeKind.New => "New",
        ChangeKind.Safer => "Safer",
        ChangeKind.Fixed => "Fixed",
        ChangeKind.Polish => "Polish",
        _ => "Changes",
    };
}

/// <summary>What changed in one release, as the person reads it.</summary>
public sealed record ReleaseNotes(string Version, bool PreRelease, DateTimeOffset? Published, string? Url, IReadOnlyList<ChangeGroup> Groups)
{
    public int Total => Groups.Sum(g => g.Items.Count);

    public int Count(ChangeKind kind) => Groups.Where(g => g.Kind == kind).Sum(g => g.Items.Count);
}

/// <summary>
/// Reads the changelog and release notes. Both use the same shape: a heading per version, then bare words (New, Safer, Fixed, Polish) that
/// start a group, then one dash line per change. Older sections that are only dash lines become one "Changes" group.
/// </summary>
public static class ChangelogParser
{
    private static readonly Dictionary<string, ChangeKind> Headings = new(StringComparer.OrdinalIgnoreCase)
    {
        ["New"] = ChangeKind.New, ["Safer"] = ChangeKind.Safer, ["Fixed"] = ChangeKind.Fixed, ["Polish"] = ChangeKind.Polish,
    };

    /// <summary>The groups in a block of text, in the order of the kinds above. Empty groups are dropped.</summary>
    public static IReadOnlyList<ChangeGroup> ParseGroups(string? text)
    {
        var items = new Dictionary<ChangeKind, List<string>>();
        var current = ChangeKind.Other;

        foreach (var raw in (text ?? "").Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (Headings.TryGetValue(line.TrimEnd(':'), out var kind)) { current = kind; continue; }
            if (!line.StartsWith("- ", StringComparison.Ordinal) && !line.StartsWith("* ", StringComparison.Ordinal)) continue;

            var item = Clean(line[2..]);
            if (item.Length == 0) continue;
            if (!items.TryGetValue(current, out var list)) items[current] = list = [];
            list.Add(item);
        }

        return Enum.GetValues<ChangeKind>().Where(items.ContainsKey).Select(k => new ChangeGroup(k, items[k])).ToList();
    }

    /// <summary>The releases in a changelog file, newest first as written. The Unreleased section is not a release and is skipped.</summary>
    public static IReadOnlyList<ReleaseNotes> ParseChangelog(string? markdown)
    {
        var sections = new List<(string Version, List<string> Lines)>();
        foreach (var raw in (markdown ?? "").Replace("\r\n", "\n").Split('\n'))
        {
            if (raw.StartsWith("## ", StringComparison.Ordinal)) { sections.Add((raw[3..].Trim(), [])); continue; }
            if (sections.Count > 0) sections[^1].Lines.Add(raw);
        }

        return sections
            .Where(s => !s.Version.Equals("Unreleased", StringComparison.OrdinalIgnoreCase) && ReleaseVersion.Parse(s.Version) is not null)
            .Select(s => new ReleaseNotes(s.Version, ReleaseVersion.Parse(s.Version)!.IsPreRelease, null, null, ParseGroups(string.Join('\n', s.Lines))))
            .ToList();
    }

    /// <summary>Backticks around commands are formatting for the web, not something to show in a window.</summary>
    private static string Clean(string item) => item.Replace("`", "").Trim();

    /// <summary>The changelog that ships inside Clarion, so the history works with no network.</summary>
    public static IReadOnlyList<ReleaseNotes> LoadEmbedded()
    {
        var asm = typeof(ChangelogParser).Assembly;
        using var stream = asm.GetManifestResourceStream("Clarion.Changelog.md");
        if (stream is null) return [];
        using var reader = new StreamReader(stream);
        return ParseChangelog(reader.ReadToEnd());
    }
}

/// <summary>Chooses what the What's new page shows.</summary>
public static class WhatsNew
{
    /// <summary>The release being run and the ones before it, newest first. Up to <paramref name="max"/>, and says how many were left out.</summary>
    public static IReadOnlyList<ReleaseNotes> InstalledAndOlder(string currentVersion, IEnumerable<ReleaseNotes> changelog, int max, out int leftOut)
    {
        var current = ReleaseVersion.Parse(currentVersion);
        var all = changelog
            .Select(n => (Notes: n, Version: ReleaseVersion.Parse(n.Version)))
            .Where(x => x.Version is not null && (current is null || x.Version <= current))
            .OrderByDescending(x => x.Version, Comparer<ReleaseVersion?>.Create((a, b) => a!.CompareTo(b)))
            .Select(x => x.Notes)
            .ToList();
        leftOut = Math.Max(0, all.Count - max);
        return all.Take(max).ToList();
    }

    /// <summary>The newer releases from GitHub as notes, newest first, with the page each one lives on.</summary>
    public static IReadOnlyList<ReleaseNotes> FromReleases(IEnumerable<ReleaseInfo> releases) =>
        releases.Select(r => new ReleaseNotes(
            r.Tag.TrimStart('v', 'V'), r.PreRelease, r.Published, UpdateCheck.IsTrustedReleaseUrl(r.Url) ? r.Url : null, ChangelogParser.ParseGroups(r.Body))).ToList();

    /// <summary>How many changes of each kind there are across the releases, for the count pills at the top.</summary>
    public static IReadOnlyDictionary<ChangeKind, int> Totals(IEnumerable<ReleaseNotes> notes)
    {
        var list = notes.ToList();
        return Enum.GetValues<ChangeKind>().Select(k => (Kind: k, Count: list.Sum(n => n.Count(k)))).Where(x => x.Count > 0).ToDictionary(x => x.Kind, x => x.Count);
    }
}
