using System.Text;
using Clarion.Core.Model;

namespace Clarion.Core.Actions;

/// <summary>Runs a tool and reports each line as it appears. Cancelling stops the tool.</summary>
public interface IStreamingRunner
{
    Task<int> RunAsync(string systemTool, string args, bool utf16, Action<string> onLine, CancellationToken cancel);
}

public sealed record ActionResult(bool Success, string? Error, long BytesFreed, TimeSpan Duration, bool Cancelled = false);

/// <summary>Runs the steps of an action in order, enforcing the tool and folder allow lists.</summary>
public sealed class ActionRunner(IStreamingRunner runner, Func<string, string>? expand = null)
{
    private readonly Func<string, string> _expand = expand ?? Environment.ExpandEnvironmentVariables;

    public async Task<ActionResult> RunAsync(ActionDef action, Action<string> log, CancellationToken cancel)
    {
        var started = DateTime.UtcNow;
        long freed = 0;
        try
        {
            foreach (var step in action.Steps)
            {
                cancel.ThrowIfCancellationRequested();
                log($"> {step.Describe()}");
                switch (step)
                {
                    case RunProcess p:
                        if (!ActionRules.IsAllowedTool(p.File)) return Fail($"{p.File} is not an allowed tool.", freed, started);
                        var code = await runner.RunAsync(p.File, p.Args, p.Utf16, log, cancel);
                        var ok = p.OkExitCodes ?? [0];
                        if (!ok.Contains(code)) return Fail($"{p.File} finished with exit code {code}.", freed, started);
                        break;
                    case CleanFolders c:
                        foreach (var folder in c.Folders)
                        {
                            if (!ActionRules.IsAllowedFolder(folder, _expand)) return Fail($"{folder} is not an allowed folder.", freed, started);
                            freed += FolderCleaner.Clean(_expand(folder), TimeSpan.FromHours(c.MinAgeHours), log);
                        }
                        break;
                    case RenameFolder r:
                        if (!ActionRules.IsAllowedFolder(r.Folder, _expand)) return Fail($"{r.Folder} is not an allowed folder.", freed, started);
                        if (!ActionRules.IsPlainName(r.NewName)) return Fail($"{r.NewName} is not a plain folder name.", freed, started);
                        FolderCleaner.Rename(_expand(r.Folder), r.NewName, log);
                        break;
                    case DeleteFolder d:
                        if (!ActionRules.IsAllowedFolder(d.Folder, _expand)) return Fail($"{d.Folder} is not an allowed folder.", freed, started);
                        freed += FolderCleaner.DeleteTree(_expand(d.Folder), log);
                        break;
                }
            }
            return new ActionResult(true, null, freed, DateTime.UtcNow - started);
        }
        catch (OperationCanceledException)
        {
            log("Stopped.");
            return new ActionResult(false, "Stopped by you.", freed, DateTime.UtcNow - started, Cancelled: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return Fail(ex.Message, freed, started);
        }
    }

    private static ActionResult Fail(string error, long freed, DateTime started) =>
        new(false, error, freed, DateTime.UtcNow - started);
}

public static class FolderCleaner
{
    /// <summary>Deletes files older than the age inside a folder. Files in use are skipped. Returns bytes freed.</summary>
    public static long Clean(string folder, TimeSpan minAge, Action<string> log)
    {
        if (!Directory.Exists(folder)) { log($"{folder} does not exist, nothing to do."); return 0; }
        var cutoff = DateTime.UtcNow - minAge;
        long freed = 0;
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };

        foreach (var file in Directory.EnumerateFiles(folder, "*", options))
        {
            try
            {
                var info = new FileInfo(file);
                if (info.LastWriteTimeUtc > cutoff) continue;
                var size = info.Length;
                info.Delete();
                freed += size;
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        foreach (var dir in Directory.EnumerateDirectories(folder, "*", options).OrderByDescending(d => d.Length))
        {
            try
            {
                if (!Directory.EnumerateFileSystemEntries(dir).Any()) Directory.Delete(dir);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        log($"Freed {Format(freed)} in {folder}");
        return freed;
    }

    public static void Rename(string folder, string newName, Action<string> log)
    {
        if (!Directory.Exists(folder)) { log($"{folder} does not exist, nothing to do."); return; }
        var parent = Path.GetDirectoryName(folder.TrimEnd('\\'))!;
        var target = Path.Combine(parent, newName);
        if (Directory.Exists(target)) throw new IOException($"{target} already exists.");
        Directory.Move(folder, target);
        log($"Renamed to {target}");
    }

    public static long DeleteTree(string folder, Action<string> log)
    {
        if (!Directory.Exists(folder)) { log($"{folder} does not exist, nothing to do."); return 0; }
        var size = Directory.EnumerateFiles(folder, "*", new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true })
            .Sum(f => { try { return new FileInfo(f).Length; } catch (IOException) { return 0L; } });
        Directory.Delete(folder, recursive: true);
        log($"Deleted {folder} and freed {Format(size)}");
        return size;
    }

    public static string Format(long bytes) => bytes switch
    {
        >= 1L << 30 => $"{bytes / (double)(1L << 30):0.0} GB",
        >= 1L << 20 => $"{bytes / (double)(1L << 20):0.0} MB",
        >= 1L << 10 => $"{bytes / (double)(1L << 10):0.0} KB",
        _ => $"{bytes} bytes",
    };
}
