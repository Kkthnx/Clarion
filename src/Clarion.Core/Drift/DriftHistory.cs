using System.Text.Json;

namespace Clarion.Core.Drift;

/// <summary>
/// A small local record of which settings each Verify scan found changed back, so a setting that Windows keeps undoing can be told from
/// one that was undone once. It is a file on this PC, read by this PC, and nothing about it is sent anywhere. Every scan is recorded, even
/// one that found nothing, because a clear scan is what ends a run of the same problem.
/// </summary>
public sealed class DriftHistory(string path)
{
    private sealed record Scan(DateTimeOffset At, int Build, string[] Ids);

    private const int KeepScans = 200;
    private static readonly JsonSerializerOptions Options = new();
    private readonly object _gate = new();

    public void Record(DateTimeOffset at, int build, IEnumerable<string> changedBack)
    {
        var line = JsonSerializer.Serialize(new Scan(at, build, changedBack.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToArray()), Options);
        lock (_gate)
        {
            try
            {
                var dir = Path.GetDirectoryName(Path.GetFullPath(path));
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                File.AppendAllText(path, line + Environment.NewLine);

                // Old scans stop mattering, and the file must not grow for ever. Trimmed now and then, not on every scan.
                var lines = File.ReadAllLines(path);
                if (lines.Length > KeepScans * 2) File.WriteAllLines(path, lines.Skip(lines.Length - KeepScans));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // History is a nicety. A folder that cannot be written, as in the limited mode, simply keeps none.
            }
        }
    }

    /// <summary>
    /// How many separate times each setting was found changed back. A setting that stays changed back across many scans counts once,
    /// until a scan finds it holding again and it is undone a second time. Only settings found changed back at least once appear.
    /// </summary>
    public IReadOnlyDictionary<string, int> TimesChangedBack() =>
        BuildsWhenChangedBack().ToDictionary(p => p.Key, p => p.Value.Count, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// For each setting, the Windows build at each separate time it was found changed back, oldest first. Comparing them says whether
    /// a Windows version change came between the times, which is what decides what is worth trying.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<int>> BuildsWhenChangedBack()
    {
        var builds = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
        var before = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var scan in ReadScans().OrderBy(s => s.At))
        {
            var now = new HashSet<string>(scan.Ids, StringComparer.OrdinalIgnoreCase);
            foreach (var id in now.Where(id => !before.Contains(id)))
            {
                if (!builds.TryGetValue(id, out var list)) builds[id] = list = [];
                list.Add(scan.Build);
            }
            before = now;
        }
        return builds.ToDictionary(p => p.Key, p => (IReadOnlyList<int>)p.Value, StringComparer.OrdinalIgnoreCase);
    }

    private List<Scan> ReadScans()
    {
        var scans = new List<Scan>();
        string[] lines;
        lock (_gate)
        {
            try
            {
                if (!File.Exists(path)) return scans;
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                using var reader = new StreamReader(stream);
                lines = reader.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return scans; }
        }

        foreach (var line in lines)
        {
            try { if (JsonSerializer.Deserialize<Scan>(line, Options) is { } scan && scan.Ids is not null) scans.Add(scan); }
            catch (JsonException) { /* a damaged line is skipped, the rest still counts */ }
        }
        return scans;
    }
}
