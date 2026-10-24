namespace Clarion.Core.Apps;

/// <summary>An installed app that has a newer version in the winget source. Name is the one winget prints, and is empty when it was not read.</summary>
public sealed record AppUpdate(string Id, string Current, string Available, string Name = "");

/// <summary>
/// Reads the table <c>winget upgrade</c> prints. The column titles and the closing sentences are in the language Windows displays, so they are
/// not used. A row is recognised by the exact package ID of an app the catalog knows, which never changes with the language, and the two words
/// after it are the installed version and the newer one. A version winget could not read in full is printed with a leading &lt;, which is dropped.
/// winget cuts a value short with an ellipsis only when its output goes to a console window (its source asks the console for its width, and a pipe
/// has none), so .Clarion, which reads a pipe, normally never sees one. If an ID does arrive cut short, it is matched by its start, and only when
/// exactly one known ID starts that way.
/// </summary>
public static class WingetUpgrades
{
    public static IReadOnlyList<AppUpdate> Parse(string text, IReadOnlySet<string> knownIds)
    {
        var updates = new List<AppUpdate>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = text.Replace("\r\n", "\n").Split('\n');

        // The table starts after the line of dashes under the column titles. Anything before it is a progress spinner or a notice.
        var start = Array.FindIndex(lines, l => l.Trim().Length >= 10 && l.Trim().All(c => c == '-'));
        if (start < 0) return updates;

        for (var i = start + 1; i < lines.Length; i++)
        {
            var words = lines[i].Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            var at = Array.FindIndex(words, w => knownIds.Contains(w));
            string? resolved = null;
            if (at < 0)
            {
                at = Array.FindIndex(words, w => (resolved = FromStart(w, knownIds)) is not null);
                if (at < 0) continue;
            }

            var rest = words.Skip(at + 1)
                .Select(w => w.TrimStart('<', '>'))
                .Where(w => w.Length > 0)
                .ToList();
            if (rest.Count < 2) continue;

            var id = resolved ?? words[at];
            if (rest[0].Equals(rest[1], StringComparison.Ordinal) || !seen.Add(id)) continue;
            updates.Add(new AppUpdate(knownIds.First(k => k.Equals(id, StringComparison.OrdinalIgnoreCase)), rest[0], rest[1]));
        }
        return updates;
    }

    /// <summary>
    /// Reads every row of the table, for any installed app and not only the ones the catalog knows. With no list of IDs to anchor on, a row is
    /// read from its right end: the last two words are the installed version and the newer one, the word before them is the package ID, and
    /// everything before that is the name. A Source word after the versions, which has no digit in it, is dropped first, and so is a sign
    /// such as &lt; before a version. A row that does not end that way is left out, which covers the closing sentences in any language and
    /// any row that cannot be read for sure, because a wrong ID would update the wrong app.
    /// </summary>
    public static IReadOnlyList<AppUpdate> ParseAll(string text)
    {
        var updates = new List<AppUpdate>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var start = Array.FindIndex(lines, l => l.Trim().Length >= 10 && l.Trim().All(c => c == '-'));
        if (start < 0) return updates;

        for (var i = start + 1; i < lines.Length; i++)
        {
            // A sign is joined to the version after it, so < 1.2 and <1.2 are read the same way.
            var words = System.Text.RegularExpressions.Regex.Replace(lines[i], @"([<>])\s+", "$1")
                .Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries).ToList();
            if (words.Count > 0 && !words[^1].Any(char.IsDigit)) words.RemoveAt(words.Count - 1);
            if (words.Count < 4) continue; // a name, an ID and two versions

            var available = words[^1].TrimStart('<', '>');
            var current = words[^2].TrimStart('<', '>');
            var id = words[^3];
            if (!current.Any(char.IsDigit) || !available.Any(char.IsDigit) || !WingetIds.IsValid(id)) continue;

            // A < before the installed version means winget could only tell that it is older than the one shown. When that is the same as the
            // newer version winget still lists the app, so it is kept and said in words, where a plain pair of equal versions is not a row.
            var olderThan = words[^2].StartsWith('<');
            if (current.Equals(available, StringComparison.Ordinal))
            {
                if (!olderThan) continue;
                current = $"older than {current}";
            }
            if (!seen.Add(id)) continue;

            updates.Add(new AppUpdate(id, current, available, string.Join(' ', words.Take(words.Count - 3))));
        }
        return updates;
    }

    /// <summary>The one known ID that a word ending in an ellipsis is the start of, or null when there is none or more than one.</summary>
    private static string? FromStart(string word, IReadOnlySet<string> knownIds)
    {
        if (word.Length < 4 || word[^1] != '…') return null;
        var start = word[..^1];
        var matches = knownIds.Where(k => k.StartsWith(start, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }
}
