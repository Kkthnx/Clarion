namespace Clarion.Core.Apps;

/// <summary>An installed app that has a newer version in the winget source.</summary>
public sealed record AppUpdate(string Id, string Current, string Available);

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

    /// <summary>The one known ID that a word ending in an ellipsis is the start of, or null when there is none or more than one.</summary>
    private static string? FromStart(string word, IReadOnlySet<string> knownIds)
    {
        if (word.Length < 4 || word[^1] != '…') return null;
        var start = word[..^1];
        var matches = knownIds.Where(k => k.StartsWith(start, StringComparison.OrdinalIgnoreCase)).Take(2).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }
}
