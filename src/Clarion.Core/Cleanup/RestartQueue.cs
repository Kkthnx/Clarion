using System.Text.Json;

namespace Clarion.Core.Cleanup;

/// <summary>Reads and writes the Windows list of file operations that run at the next restart.</summary>
public interface IPendingRenameStore
{
    /// <summary>The raw entries, in order. Each operation is two entries: the file, then where it goes. An empty second entry means delete.</summary>
    IReadOnlyList<string> Read();

    /// <summary>Replaces the list. An empty list removes the value.</summary>
    void Write(IReadOnlyList<string> entries);
}

public sealed record QueuedFile(string Path, DateTimeOffset Time);

/// <summary>What is waiting for a restart. Other programs queue work in the same Windows list, which Clarion only counts.</summary>
public sealed record QueueView(IReadOnlyList<QueuedFile> Waiting, IReadOnlyList<QueuedFile> NoLongerQueued, int OtherOperations);

/// <summary>
/// Pure rules for the Windows restart list. Delete operations are stored as a path, often written with a
/// \??\ prefix and sometimes a leading marker, followed by an empty entry.
/// </summary>
public static class PendingDeletes
{
    private const string NtPrefix = "\\??\\";

    public static string Normalize(string entry)
    {
        var i = entry.IndexOf(NtPrefix, StringComparison.Ordinal);
        return (i >= 0 ? entry[(i + NtPrefix.Length)..] : entry).Trim();
    }

    private static bool IsDelete(IReadOnlyList<string> raw, int index) =>
        index + 1 >= raw.Count || string.IsNullOrEmpty(raw[index + 1]);

    /// <summary>The files set to be deleted at restart, in the Windows list order.</summary>
    public static IReadOnlyList<string> DeleteTargets(IReadOnlyList<string> raw)
    {
        var files = new List<string>();
        for (var i = 0; i < raw.Count; i += 2)
            if (IsDelete(raw, i)) files.Add(Normalize(raw[i]));
        return files;
    }

    public static int OperationCount(IReadOnlyList<string> raw) => (raw.Count + 1) / 2;

    /// <summary>Drops only the delete operations for the given files. Everything else keeps its entries and its order.</summary>
    public static (IReadOnlyList<string> Entries, int Removed) Remove(IReadOnlyList<string> raw, IReadOnlySet<string> files)
    {
        var kept = new List<string>();
        var removed = 0;
        for (var i = 0; i < raw.Count; i += 2)
        {
            var isDelete = IsDelete(raw, i);
            if (isDelete && files.Contains(Normalize(raw[i]))) { removed++; continue; }
            kept.Add(raw[i]);
            kept.Add(i + 1 < raw.Count ? raw[i + 1] : "");
        }
        return (kept, removed);
    }
}

/// <summary>
/// Keeps a record of the files Clarion asked Windows to delete at the next restart, so they can be listed
/// and cancelled. Records add up across runs. Cancelling touches only Clarion's own operations.
/// </summary>
public sealed class RestartQueue(string ledgerPath, IPendingRenameStore store, TimeProvider? clock = null)
{
    private readonly TimeProvider _clock = clock ?? TimeProvider.System;
    private readonly object _gate = new();

    private static readonly StringComparer Same = StringComparer.OrdinalIgnoreCase;

    private List<QueuedFile> Load()
    {
        try
        {
            if (!File.Exists(ledgerPath)) return [];
            return JsonSerializer.Deserialize<List<QueuedFile>>(File.ReadAllText(ledgerPath)) ?? [];
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException) { return []; }
    }

    private void Save(List<QueuedFile> items)
    {
        try
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(ledgerPath));
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            if (items.Count == 0) File.Delete(ledgerPath);
            else File.WriteAllText(ledgerPath, JsonSerializer.Serialize(items));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }

    /// <summary>Notes files that were just queued. Files already on the record keep their first time.</summary>
    public void Record(IEnumerable<string> files)
    {
        lock (_gate)
        {
            var items = Load();
            var now = _clock.GetUtcNow();
            foreach (var f in files.Select(PendingDeletes.Normalize).Where(f => f.Length > 0).Distinct(Same))
                if (!items.Any(i => Same.Equals(i.Path, f))) items.Add(new QueuedFile(f, now));
            Save(items);
        }
    }

    public QueueView View()
    {
        lock (_gate)
        {
            var items = Load();
            var raw = store.Read();
            var inWindows = PendingDeletes.DeleteTargets(raw).ToHashSet(Same);
            var waiting = items.Where(i => inWindows.Contains(i.Path)).ToList();
            var gone = items.Where(i => !inWindows.Contains(i.Path)).ToList();
            return new QueueView(waiting, gone, Math.Max(0, PendingDeletes.OperationCount(raw) - waiting.Count));
        }
    }

    /// <summary>Takes Clarion's files off the Windows restart list. Returns how many were removed. Other programs' work is left in place.</summary>
    public int Cancel()
    {
        lock (_gate)
        {
            var items = Load();
            var raw = store.Read();
            var ours = items.Select(i => i.Path).ToHashSet(Same);
            var (entries, removed) = PendingDeletes.Remove(raw, ours);
            if (removed > 0) store.Write(entries);
            Save([]);
            return removed;
        }
    }

    /// <summary>
    /// After a restart, Windows has run the list. Drops the files queued before that boot from the record and
    /// says how many of them are still on disk.
    /// </summary>
    public (int Processed, int StillThere) Settle(DateTime bootUtc, Func<string, bool> exists)
    {
        lock (_gate)
        {
            var items = Load();
            var done = items.Where(i => i.Time.UtcDateTime < bootUtc).ToList();
            if (done.Count == 0) return (0, 0);
            Save(items.Except(done).ToList());
            return (done.Count, done.Count(i => exists(i.Path)));
        }
    }
}
