namespace Clarion.Core.Cleanup;

/// <summary>Scans and cleans cleanup rows. Counts only files that were really deleted.</summary>
public sealed class CleanupEngine(ICleanupPlatform platform)
{
    public TargetScan Scan(CleanTarget target, CancellationToken cancel = default)
    {
        var running = RunningFor(target);
        var folders = new List<RuleScan>();
        long bytes = 0;
        var files = 0;

        foreach (var rule in target.Rules)
        {
            switch (rule)
            {
                case RecycleBinRule:
                    bytes += platform.RecycleBinBytes();
                    folders.Add(new RuleScan("Recycle Bin", platform.RecycleBinBytes(), 0, true));
                    break;
                case EventLogsRule:
                    var count = platform.EventLogNames().Count(n => !IsSecurityLog(n));
                    folders.Add(new RuleScan($"{count} event logs", 0, count, count > 0));
                    files += count;
                    break;
                default:
                    foreach (var (folder, pattern, trusted) in Resolve(rule))
                    {
                        cancel.ThrowIfCancellationRequested();
                        if (!Directory.Exists(folder) || !PathGuard.IsSafe(folder, platform, trusted))
                        {
                            folders.Add(new RuleScan(folder, 0, 0, false));
                            continue;
                        }
                        var (b, f) = Measure(folder, pattern, target.MinAgeHours, cancel);
                        folders.Add(new RuleScan(folder, b, f, true));
                        bytes += b;
                        files += f;
                    }
                    break;
            }
        }
        return new TargetScan(target.Id, folders, bytes, files, running);
    }

    public TargetClean Clean(CleanTarget target, bool queueLocked, bool preview, Action<string>? currentFile, CancellationToken cancel)
    {
        var running = RunningFor(target);
        if (running.Count > 0)
            return new TargetClean(target.Id, 0, 0, 0, 0, [$"Close {string.Join(", ", running)} first."], Skipped: true);

        long freed = 0;
        var deleted = 0;
        var locked = 0;
        var queued = 0;
        var notes = new List<string>();

        foreach (var rule in target.Rules)
        {
            cancel.ThrowIfCancellationRequested();
            switch (rule)
            {
                case RecycleBinRule when !preview:
                    freed += platform.RecycleBinBytes();
                    platform.EmptyRecycleBin();
                    deleted++;
                    break;
                case EventLogsRule when !preview:
                    foreach (var name in platform.EventLogNames().Where(n => !IsSecurityLog(n)))
                    {
                        cancel.ThrowIfCancellationRequested();
                        try { platform.ClearEventLog(name); deleted++; }
                        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException) { locked++; }
                    }
                    break;
                case RecycleBinRule or EventLogsRule:
                    break;
                default:
                    foreach (var (folder, pattern, trusted) in Resolve(rule))
                    {
                        if (!Directory.Exists(folder)) continue;
                        if (!PathGuard.IsSafe(folder, platform, trusted))
                        {
                            notes.Add($"Skipped {folder} because it is not on the allow list or is a link.");
                            continue;
                        }
                        var r = CleanFolder(folder, pattern, target.MinAgeHours, queueLocked && target.QueueLockedAtRestart, preview, currentFile, cancel);
                        freed += r.Freed;
                        deleted += r.Deleted;
                        locked += r.Locked;
                        queued += r.Queued;
                    }
                    break;
            }
        }

        if (locked > 0 && queued == 0) notes.Add($"{locked} files are in use and were left alone.");
        if (queued > 0) notes.Add($"{queued} files will be removed at the next restart.");
        return new TargetClean(target.Id, freed, deleted, locked, queued, notes);
    }

    private IEnumerable<(string Folder, string Pattern, bool Trusted)> Resolve(CleanRule rule)
    {
        switch (rule)
        {
            case FolderRule f:
                var expanded = platform.Expand(f.Path);
                var last = Path.GetFileName(expanded);
                if (last.Contains('*', StringComparison.Ordinal))
                {
                    var parent = Path.GetDirectoryName(expanded);
                    if (parent is not null && Directory.Exists(parent))
                    {
                        foreach (var match in Directory.EnumerateDirectories(parent, last)) yield return (match, "*", false);
                    }
                }
                else
                {
                    yield return (expanded, "*", false);
                }
                break;
            case FilePatternRule p:
                yield return (platform.Expand(p.Folder), p.Pattern, false);
                break;
            case SteamShaderRule:
                foreach (var lib in platform.SteamLibraries())
                    yield return (Path.Combine(lib, "steamapps", "shadercache"), "*", Directory.Exists(Path.Combine(lib, "steamapps")));
                break;
            case WowCacheRule:
                foreach (var version in platform.WowVersionFolders())
                    yield return (Path.Combine(version, "Cache"), "*", IsWowFolder(version));
                break;
        }
    }

    private static bool IsWowFolder(string folder) =>
        Directory.Exists(folder) && Directory.EnumerateFiles(folder, "Wow*.exe").Any();

    private List<string> RunningFor(CleanTarget target)
    {
        if (target.Processes.Count == 0) return [];
        var running = platform.RunningProcesses();
        return target.Processes.Where(p => running.Contains(p, StringComparer.OrdinalIgnoreCase)).ToList();
    }

    private static bool IsSecurityLog(string name) => name.Equals("Security", StringComparison.OrdinalIgnoreCase);

    private static (long Bytes, int Files) Measure(string folder, string pattern, int minAgeHours, CancellationToken cancel)
    {
        long bytes = 0;
        var count = 0;
        var cutoff = minAgeHours > 0 ? DateTime.UtcNow.AddHours(-minAgeHours) : DateTime.MaxValue;
        foreach (var file in Walk(folder, pattern))
        {
            cancel.ThrowIfCancellationRequested();
            try
            {
                var info = new FileInfo(file);
                if (minAgeHours > 0 && info.LastWriteTimeUtc > cutoff) continue;
                bytes += info.Length;
                count++;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        return (bytes, count);
    }

    /// <summary>Files under a folder. Links are never followed.</summary>
    private static IEnumerable<string> Walk(string folder, string pattern)
    {
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
        return Directory.EnumerateFiles(folder, pattern, options);
    }

    private (long Freed, int Deleted, int Locked, int Queued) CleanFolder(
        string folder, string pattern, int minAgeHours, bool queueLocked, bool preview, Action<string>? currentFile, CancellationToken cancel)
    {
        long freed = 0;
        var deleted = 0;
        var locked = 0;
        var queued = 0;
        var cutoff = minAgeHours > 0 ? DateTime.UtcNow.AddHours(-minAgeHours) : DateTime.MaxValue;

        foreach (var file in Walk(folder, pattern).ToList())
        {
            cancel.ThrowIfCancellationRequested();
            try
            {
                var info = new FileInfo(file);
                if (minAgeHours > 0 && info.LastWriteTimeUtc > cutoff) continue;
                currentFile?.Invoke(file);
                var size = info.Length;
                if (preview) { freed += size; deleted++; continue; }

                if ((info.Attributes & FileAttributes.ReadOnly) != 0) info.Attributes &= ~FileAttributes.ReadOnly;
                info.Delete();
                freed += size;
                deleted++;
            }
            catch (IOException)
            {
                locked++;
                if (queueLocked && platform.QueueDeleteAtRestart(file)) queued++;
            }
            catch (UnauthorizedAccessException)
            {
                locked++;
                if (queueLocked && platform.QueueDeleteAtRestart(file)) queued++;
            }
        }

        if (!preview && pattern == "*")
        {
            RemoveLinksAndEmptyFolders(folder);
        }
        return (freed, deleted, locked, queued);
    }

    /// <summary>Links inside a cache are removed without opening them. Empty folders go, the root stays.</summary>
    private static void RemoveLinksAndEmptyFolders(string root)
    {
        foreach (var dir in Directory.EnumerateDirectories(root, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, ReturnSpecialDirectories = false })
                     .OrderByDescending(d => d.Length).ToList())
        {
            try
            {
                var attrs = File.GetAttributes(dir);
                if ((attrs & FileAttributes.ReparsePoint) != 0)
                {
                    Directory.Delete(dir, recursive: false);
                    continue;
                }
                if (!Directory.EnumerateFileSystemEntries(dir).Any()) Directory.Delete(dir);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
