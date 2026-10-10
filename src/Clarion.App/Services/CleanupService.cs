using Clarion.Core.Model;
using Clarion.Core.Actions;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using Clarion.Core.Catalog;
using Clarion.Core.Cleanup;
using Clarion.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clarion.App.Services;

public sealed record CleanupSummary(long BytesFreed, int FilesDeleted, int FilesLocked, int Queued, TimeSpan Duration, bool Preview, bool Cancelled);

/// <summary>Runs scans and cleans for the cleanup page and keeps the restart queue record.</summary>
public sealed class CleanupService : UiObservableObject
{
    public static CleanupService Instance { get; } = new();

    private readonly WindowsCleanupPlatform _platform = new();
    private readonly CleanupEngine _engine;
    private CancellationTokenSource? _cts;
    private bool _busy;
    private string _status = "Press Scan to see what can be cleaned";
    private string _currentFile = "";
    private long _freedLive;
    private int _filesLive;

    /// <summary>Files Clarion has asked Windows to delete at the next restart, with the way to see and cancel them.</summary>
    public RestartQueue Queue { get; } = new(Path.Combine(EngineFactory.DefaultDataDirectory, "restart-queue.json"), new WindowsPendingRenameStore());

    /// <summary>Files the last run set for restart, or in a preview the files that would be.</summary>
    public IReadOnlyList<string> LastQueuedFiles { get; private set; } = [];
    public bool LastWasPreview { get; private set; }

    private static string ReportFile => Path.Combine(EngineFactory.DefaultDataDirectory, "last-cleanup.txt");

    private CleanupService()
    {
        _engine = new CleanupEngine(_platform);
        foreach (var t in CatalogLoader.LoadCleanup()) Items.Add(new CleanupItem(t));
    }

    public ObservableCollection<CleanupItem> Items { get; } = [];

    public bool IsBusy { get => _busy; private set => SetProperty(ref _busy, value); }
    public string Status { get => _status; private set => SetProperty(ref _status, value); }
    public string CurrentFile { get => _currentFile; private set => SetProperty(ref _currentFile, value); }
    public long FreedLive { get => _freedLive; private set => SetProperty(ref _freedLive, value); }
    public int FilesLive { get => _filesLive; private set => SetProperty(ref _filesLive, value); }
    public CleanupSummary? LastSummary { get; private set; }

    public long SelectedBytes => Items.Where(i => i.IsChecked && i.CanSelect).Sum(i => i.ScannedBytes);

    public async Task ScanAsync()
    {
        IsBusy = true;
        Status = "Scanning";
        using var cts = new CancellationTokenSource();
        _cts = cts;
        try
        {
            await Task.Run(() =>
            {
                foreach (var item in Items)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    var scan = _engine.Scan(item.Target, cts.Token);
                    item.Scan = scan;
                    item.Result = null;
                    if (!item.CanSelect) item.IsChecked = false;
                }
            });
            var total = Items.Where(i => i.IsPresent).Sum(i => i.ScannedBytes);
            Status = $"Found {ByteSize.Format(total)} that can be cleaned";
        }
        catch (OperationCanceledException)
        {
            Status = "Scan stopped";
        }
        finally
        {
            _cts = null;
            IsBusy = false;
            OnPropertyChanged(nameof(SelectedBytes));
        }
    }

    public void SelectSuggested()
    {
        foreach (var i in Items) i.IsChecked = i.Target.DefaultOn && i.CanSelect;
        OnPropertyChanged(nameof(SelectedBytes));
    }

    public void Cancel() => _cts?.Cancel();

    public async Task<CleanupSummary> CleanAsync(bool preview, bool queueDriverFiles)
    {
        IsBusy = true;
        FreedLive = 0;
        FilesLive = 0;
        using var cts = new CancellationTokenSource();
        _cts = cts;
        var started = DateTime.UtcNow;
        var expert = AppServices.Instance.ExpertMode;
        var chosen = Items.Where(i => i.IsChecked && i.CanSelect && (expert || !i.Target.ExpertOnly)).ToList();
        Status = preview ? "Measuring" : "Cleaning";
        var queuedFiles = new List<string>();
        long freed = 0;
        int deleted = 0, locked = 0, queued = 0;
        var cancelled = false;
        var lastUi = DateTime.UtcNow;

        try
        {
            await Task.Run(() =>
            {
                foreach (var item in chosen)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    item.IsBusy = true;
                    var seen = 0;
                    var result = _engine.Clean(item.Target, queueDriverFiles, preview, file =>
                    {
                        if (++seen % 25 != 0 && (DateTime.UtcNow - lastUi).TotalMilliseconds < 120) return;
                        lastUi = DateTime.UtcNow;
                        CurrentFile = file;
                        FilesLive = deleted + seen;
                    }, cts.Token);
                    item.IsBusy = false;
                    item.Result = result;
                    freed += result.BytesFreed;
                    deleted += result.FilesDeleted;
                    locked += result.FilesLocked;
                    queued += result.QueuedForRestart;
                    queuedFiles.AddRange(result.QueuedFiles);
                    FreedLive = freed;
                    FilesLive = deleted;
                    Log.Write($"cleanup {item.Id}: {(preview ? "preview " : "")}{ByteSize.Format(result.BytesFreed)}, {result.FilesDeleted} files, {result.FilesLocked} in use, {result.QueuedForRestart} queued{(result.Skipped ? ", skipped" : "")}");
                }
            });
        }
        catch (OperationCanceledException)
        {
            cancelled = true;
        }
        finally
        {
            _cts = null;
            foreach (var i in Items) i.IsBusy = false;
            IsBusy = false;
            CurrentFile = "";
        }

        if (!preview && queuedFiles.Count > 0) Queue.Record(queuedFiles);
        LastQueuedFiles = queuedFiles;
        LastWasPreview = preview;
        var summary = new CleanupSummary(freed, deleted, locked, queued, DateTime.UtcNow - started, preview, cancelled);
        LastSummary = summary;
        Status = cancelled ? "Stopped. What is already gone stays gone."
            : preview ? $"Would free {ByteSize.Format(freed)}"
            : $"Freed {ByteSize.Format(freed)}";
        if (!preview) SaveReport(chosen, summary);
        OnPropertyChanged(nameof(SelectedBytes));
        return summary;
    }

    public string BuildReport(IEnumerable<CleanupItem> rows, CleanupSummary s)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Clarion cleanup {DateTime.Now:yyyy-MM-dd HH:mm}{(s.Preview ? " (preview)" : "")}");
        sb.AppendLine($"Freed {ByteSize.Format(s.BytesFreed)} from {s.FilesDeleted} files in {s.Duration.TotalSeconds:0.0} s");
        if (s.FilesLocked > 0) sb.AppendLine($"{s.FilesLocked} files were in use and left alone");
        if (s.Queued > 0) sb.AppendLine($"{s.Queued} files are queued for removal at the next restart");
        sb.AppendLine();
        foreach (var r in rows.Where(r => r.Result is not null))
            sb.AppendLine($"{r.Name,-34} {ByteSize.Format(r.Result!.BytesFreed),10}  {string.Join(" ", r.Result.Notes)}");
        return sb.ToString();
    }

    private void SaveReport(IEnumerable<CleanupItem> rows, CleanupSummary s)
    {
        try
        {
            Directory.CreateDirectory(EngineFactory.DefaultDataDirectory);
            File.WriteAllText(ReportFile, BuildReport(rows, s));
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    public string LastReportText() => File.Exists(ReportFile) ? File.ReadAllText(ReportFile) : "";

    // ---- restart queue ----

    /// <summary>After a restart, says what Windows did with the files that were queued. Before one, says how many are still waiting.</summary>
    public string? CheckQueuedFromLastTime()
    {
        try
        {
            var (processed, stillThere) = Queue.Settle(BootTime(), File.Exists);
            if (processed > 0)
                return stillThere == 0
                    ? $"After the restart, Windows removed all {processed} queued files."
                    : $"After the restart, {processed - stillThere} of {processed} queued files were removed and {stillThere} are still there.";
            var waiting = Queue.View().Waiting.Count;
            return waiting > 0 ? $"{waiting} files are still waiting for a restart to be removed. You can review or cancel them below." : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            return null;
        }
    }

    private static DateTime BootTime() => DateTime.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64);
}
