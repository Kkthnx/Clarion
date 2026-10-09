using System.Text.Json;
using System.Text.Json.Serialization;
using Clarion.Core.Model;

namespace Clarion.Core.Journal;

/// <summary>Release ends tracking of a tweak without changing Windows, for when the user accepts a change that was made outside Clarion.</summary>
public enum JournalAction { Apply, Revert, Release }

/// <summary>One executed step. Undo is the operation that puts the prior state back.</summary>
public sealed record JournalEntry(
    Guid BatchId,
    string TweakId,
    JournalAction Action,
    DateTimeOffset Time,
    Operation Operation,
    Operation Undo);

/// <summary>Append-only JSON lines file recording every change with the operation that reverses it.</summary>
public sealed class ChangeJournal
{
    private static readonly JsonSerializerOptions Options = new()
    {
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string _path;
    private readonly object _gate = new();

    public ChangeJournal(string path)
    {
        _path = path;
        var dir = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
    }

    public void Append(JournalEntry entry)
    {
        var line = JsonSerializer.Serialize(entry, Options) + Environment.NewLine;
        lock (_gate)
        {
            using var fs = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
            using var w = new StreamWriter(fs);
            if (EndsWithPartialLine()) w.Write(Environment.NewLine);
            w.Write(line);
            w.Flush();
            fs.Flush(true);
        }
    }

    /// <summary>How many damaged lines the last read skipped, for example after a power cut mid write.</summary>
    public int SkippedLines { get; private set; }

    private bool EndsWithPartialLine()
    {
        if (!File.Exists(_path)) return false;
        using var fs = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (fs.Length == 0) return false;
        fs.Seek(-1, SeekOrigin.End);
        return fs.ReadByte() != (int)'\n';
    }

    public IReadOnlyList<JournalEntry> ReadAll()
    {
        lock (_gate)
        {
            if (!File.Exists(_path)) return [];
            var list = new List<JournalEntry>();
            SkippedLines = 0;
            foreach (var line in File.ReadLines(_path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var e = JsonSerializer.Deserialize<JournalEntry>(line, Options);
                    if (e is not null) list.Add(e);
                }
                catch (JsonException)
                {
                    SkippedLines++;
                }
            }
            return list;
        }
    }

    /// <summary>Where the latest apply of each tweak sits in the journal, so which of two was applied later can be told exactly.</summary>
    public IReadOnlyDictionary<string, int> LastApplyOrder()
    {
        var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var all = ReadAll();
        for (var i = 0; i < all.Count; i++)
            if (all[i].Action == JournalAction.Apply) order[all[i].TweakId] = i;
        return order;
    }

    /// <summary>The recorded steps of every tweak that is still on, keyed by tweak id. Reads the file once.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<JournalEntry>> OutstandingByTweak()
    {
        var result = new Dictionary<string, IReadOnlyList<JournalEntry>>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in ReadAll().GroupBy(e => e.TweakId))
        {
            var list = group.ToList();
            var lastApply = list.FindLastIndex(e => e.Action == JournalAction.Apply);
            if (lastApply < 0 || list.Skip(lastApply + 1).Any(e => e.Action != JournalAction.Apply)) continue;
            var batch = list[lastApply].BatchId;
            result[group.Key] = list.Where(e => e.Action == JournalAction.Apply && e.BatchId == batch).ToList();
        }
        return result;
    }

    /// <summary>Ids of every tweak that has applied changes not yet reverted. Reads the file once.</summary>
    public IReadOnlySet<string> TweakIdsWithOutstanding()
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var group in ReadAll().GroupBy(e => e.TweakId))
        {
            var list = group.ToList();
            var lastApply = list.FindLastIndex(e => e.Action == JournalAction.Apply);
            if (lastApply >= 0 && !list.Skip(lastApply + 1).Any(e => e.Action != JournalAction.Apply)) result.Add(group.Key);
        }
        return result;
    }

    /// <summary>
    /// Entries of the latest apply batch for a tweak that has not been reverted since,
    /// in the order they were applied. Empty when nothing is outstanding.
    /// </summary>
    public IReadOnlyList<JournalEntry> OutstandingFor(string tweakId)
    {
        var mine = ReadAll().Where(e => e.TweakId == tweakId).ToList();
        var lastApplyIndex = mine.FindLastIndex(e => e.Action == JournalAction.Apply);
        if (lastApplyIndex < 0) return [];
        var batch = mine[lastApplyIndex].BatchId;
        var revertedAfter = mine.Skip(lastApplyIndex + 1).Any(e => e.Action != JournalAction.Apply);
        if (revertedAfter) return [];
        return mine.Where(e => e.Action == JournalAction.Apply && e.BatchId == batch).ToList();
    }
}
