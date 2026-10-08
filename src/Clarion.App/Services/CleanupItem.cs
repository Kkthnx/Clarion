using Clarion.Core.Actions;
using Clarion.App.Controls;
using Clarion.Core.Cleanup;
using Clarion.Core.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clarion.App.Services;

/// <summary>One row of the cleanup list with its scan result and tick box.</summary>
public sealed class CleanupItem : UiObservableObject, IDetailSource
{
    private bool _isChecked;
    private TargetScan? _scan;
    private TargetClean? _result;
    private bool _busy;

    public CleanupItem(CleanTarget target)
    {
        Target = target;
        _isChecked = target.DefaultOn;
        Tooltip = TweakTooltip.BuildItem(
            target.Name, target.Summary, target.Recommendation, target.Advice, target.Facts, target.Benefit, target.Risk,
            target.RiskLevel, target.Irreversible ? "Cannot be undone" : "Only files that are rebuilt on demand",
            BuildNotes(target));
    }

    public CleanTarget Target { get; }
    public TooltipContent Tooltip { get; }

    public string Id => Target.Id;
    public string Group => Target.Group;
    public string Name => Target.Name;
    public string Summary => Target.Summary;
    public string RecommendationText => Tooltip.RecommendationLabel;
    public string RiskText => DetailHelpers.RiskText(Target.RiskLevel);
    public string EvidenceText => Tooltip.EvidenceLabel;
    public IReadOnlyList<string> Provenance => [];
    public IReadOnlyList<SourceLink> SourceLinks => DetailHelpers.Links(Target.Sources);

    public ChipKind RecommendationKind => DetailHelpers.RecommendationKind(Target.Recommendation);
    public ChipKind RiskKind => DetailHelpers.RiskKind(Target.RiskLevel);

    /// <summary>The folders a scan found, so people can see exactly what will be touched.</summary>
    public IReadOnlyList<string> ExactChanges =>
        _scan is null
            ? Target.Rules.Select(DescribeRule).ToList()
            : _scan.Folders.Select(f => f.Exists ? $"{f.Folder}  ({ByteSize.Format(f.Bytes)}, {f.Files} files)" : $"{f.Folder}  (not on this PC)").ToList();

    public bool IsChecked
    {
        get => _isChecked;
        set { if (SetProperty(ref _isChecked, value)) CheckedChanged?.Invoke(this, EventArgs.Empty); }
    }

    /// <summary>CheckBox binds to a nullable value, so this adapts it.</summary>
    public bool? CheckedBinding
    {
        get => IsChecked;
        set => IsChecked = value ?? false;
    }

    public event EventHandler? CheckedChanged;

    public TargetScan? Scan
    {
        get => _scan;
        set
        {
            if (!SetProperty(ref _scan, value)) return;
            OnPropertyChanged(nameof(SizeText));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(CanSelect));
            OnPropertyChanged(nameof(ExactChanges));
            OnPropertyChanged(nameof(IsPresent));
        }
    }

    public TargetClean? Result
    {
        get => _result;
        set
        {
            if (!SetProperty(ref _result, value)) return;
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(SizeText));
        }
    }

    public bool IsBusy { get => _busy; set { if (SetProperty(ref _busy, value)) OnPropertyChanged(nameof(StatusText)); } }

    public bool IsPresent => _scan is null || _scan.Present;
    public bool CanSelect => _scan is null || (_scan.Present && _scan.RunningApps.Count == 0);

    public long ScannedBytes => _scan?.Bytes ?? 0;

    public string SizeText
    {
        get
        {
            if (_result is { Skipped: false }) return $"{ByteSize.Format(_result.BytesFreed)} freed";
            if (_scan is null) return "";
            if (!_scan.Present) return "";
            return _scan.Bytes == 0 ? "Empty" : ByteSize.Format(_scan.Bytes);
        }
    }

    public string StatusText
    {
        get
        {
            if (_busy) return "Cleaning";
            if (_result is not null) return string.Join(" ", _result.Notes.Count > 0 ? _result.Notes : [$"{_result.FilesDeleted} files removed."]);
            if (_scan is null) return "";
            if (_scan.RunningApps.Count > 0) return $"Close {string.Join(", ", _scan.RunningApps)} first";
            return _scan.Present ? $"{_scan.Files} files" : "Not on this PC";
        }
    }

    private static IReadOnlyList<string> BuildNotes(CleanTarget t)
    {
        var notes = new List<string>();
        if (t.Processes.Count > 0) notes.Add($"Skipped while {t.Processes[0]} is running, so no open file is touched.");
        if (t.MinAgeHours > 0) notes.Add($"Only files untouched for {t.MinAgeHours} hours are removed.");
        if (t.QueueLockedAtRestart) notes.Add("Files held open by the driver are removed at the next restart.");
        notes.Add("Clarion shows a preview first and counts only files it really deleted.");
        return notes;
    }

    private static string DescribeRule(CleanRule rule) => rule switch
    {
        FolderRule f => f.Path,
        FilePatternRule p => $"{p.Folder}\\{p.Pattern}",
        SteamShaderRule => "steamapps\\shadercache in every Steam library",
        WowCacheRule => "Cache folder of each World of Warcraft version",
        RecycleBinRule => "The Recycle Bin on every drive",
        EventLogsRule => "Classic Windows event logs, never Security",
        _ => rule.GetType().Name,
    };
}
