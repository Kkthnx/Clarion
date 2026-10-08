using System.Text.Json;
using System.Text.Json.Serialization;
using Clarion.Core.Model;

namespace Clarion.Core.Journal;

public enum JournalAction { Apply, Revert }

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
            w.Write(line);
            w.Flush();
            fs.Flush(true);
        }
    }

    public IReadOnlyList<JournalEntry> ReadAll()
    {
        lock (_gate)
        {
            if (!File.Exists(_path)) return [];
            var list = new List<JournalEntry>();
            foreach (var line in File.ReadLines(_path))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var e = JsonSerializer.Deserialize<JournalEntry>(line, Options);
                if (e is not null) list.Add(e);
            }
            return list;
        }
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
        var revertedAfter = mine.Skip(lastApplyIndex + 1).Any(e => e.Action == JournalAction.Revert);
        if (revertedAfter) return [];
        return mine.Where(e => e.Action == JournalAction.Apply && e.BatchId == batch).ToList();
    }
}
