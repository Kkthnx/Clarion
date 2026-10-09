using Clarion.App.Controls;
using Clarion.Core.Appx;
using Clarion.Core.Model;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clarion.App.Services;

/// <summary>One catalog entry as the UI sees it: its live state, what the user asked for, and why it may be locked.</summary>
public sealed partial class TweakItem : UiObservableObject, IDetailSource
{
    public TweakItem(Tweak tweak, MachineProfile profile)
    {
        Tweak = tweak;
        Tooltip = TweakTooltip.Build(tweak);
        IsSupported = profile.Supports(tweak.Requires);
        LockReason = IsSupported ? "" : BuildLockReason(tweak, profile);
    }

    internal Tweak Tweak { get; }
    public TooltipContent Tooltip { get; }
    public bool IsSupported { get; }
    public string LockReason { get; }

    public string RecommendationText => Tooltip.RecommendationLabel;
    public string RiskText => DetailHelpers.RiskText(Tweak.RiskLevel);
    public string EvidenceText => Tooltip.EvidenceLabel;
    public IReadOnlyList<string> ExactChanges => EffectiveTweak.Apply.Select(o => o.Describe()).ToList();
    public IReadOnlyList<SourceLink> SourceLinks => DetailHelpers.Links(Tweak.Sources);
    public IReadOnlyList<string> Provenance => Tweak.Provenance;
    public bool HasSources => Tweak.Sources.Count > 0;
    public bool HasLastError => LastError.Length > 0;

    public ChipKind RecommendationKind => DetailHelpers.RecommendationKind(Tweak.Recommendation);
    public ChipKind RiskKind => DetailHelpers.RiskKind(Tweak.RiskLevel);

    /// <summary>Expert option for app removals: also stop Windows installing the app for accounts created later.</summary>
    public bool CanDeprovision => IsSupported && DeprovisionVariant.CanApply(Tweak);

    public bool DeprovisionToo { get; private set; }

    /// <summary>True once the journal shows this was applied with the option, which Revert cannot undo.</summary>
    public bool DeprovisionLocked { get; private set; }

    /// <summary>What actually runs: the catalog entry, or its variant when the option is on.</summary>
    internal Tweak EffectiveTweak => DeprovisionToo ? DeprovisionVariant.Of(Tweak) : Tweak;

    public void SetDeprovision(bool on, bool locked)
    {
        DeprovisionToo = on;
        DeprovisionLocked = locked;
        OnPropertyChanged(nameof(DeprovisionToo));
        OnPropertyChanged(nameof(DeprovisionLocked));
        OnPropertyChanged(nameof(ExactChanges));
    }

    public string Id => Tweak.Id;
    public string Name => Tweak.Name;
    public string Summary => Tweak.Summary;
    public string Category => Tweak.Category;

    private bool _syncing;
    private bool _isChecking = true;
    private bool _isDrifted;
    private TweakState _state = TweakState.NotApplied;
    private bool _isOn;
    private string _lastError = "";

    public TweakState State
    {
        get => _state;
        set
        {
            if (!SetProperty(ref _state, value)) return;
            OnPropertyChanged(nameof(CanToggle));
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(StateText));
            OnPropertyChanged(nameof(IsApplied));
            OnPropertyChanged(nameof(IsUnavailable));
            OnPropertyChanged(nameof(PendingText));
        }
    }

    public bool IsOn
    {
        get => _isOn;
        set
        {
            if (!SetProperty(ref _isOn, value)) return;
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(PendingText));
            if (!_syncing) PendingChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public string LastError
    {
        get => _lastError;
        set
        {
            if (SetProperty(ref _lastError, value)) OnPropertyChanged(nameof(HasLastError));
        }
    }

    public bool IsLocked => !IsSupported;
    /// <summary>True while the real state is still being read.</summary>
    public bool IsChecking
    {
        get => _isChecking;
        private set
        {
            if (!SetProperty(ref _isChecking, value)) return;
            OnPropertyChanged(nameof(CanToggle));
            OnPropertyChanged(nameof(StateText));
        }
    }

    /// <summary>True when Clarion applied this earlier but Windows has since changed it back.</summary>
    public bool IsDrifted { get => _isDrifted; set => SetProperty(ref _isDrifted, value); }

    /// <summary>Registry only tweaks read in milliseconds. Services, apps and features need PowerShell.</summary>
    public bool ReadsQuickly => Tweak.Apply.All(o => o is SetRegistryValue or DeleteRegistryValue);

    public bool IsApplied => State == TweakState.Applied;
    public bool IsUnavailable => State == TweakState.Unavailable;
    public bool CanToggle => IsSupported && !IsChecking && State != TweakState.Unavailable;
    public bool IsPending => CanToggle && IsOn != IsApplied;

    public string StateText => !IsSupported ? "Not on this PC"
        : IsChecking ? "Checking"
        : IsDrifted ? "Changed back by Windows"
        : State switch
        {
            TweakState.Applied => "On",
            TweakState.Partial => "Partly on",
            TweakState.Unavailable => "Nothing to change here",
            _ => "Off",
        };

    /// <summary>What happens when the pending change is run.</summary>
    public string PendingText => IsPending ? (IsOn ? "Will be turned on" : "Will be reverted") : "";


    public event EventHandler? PendingChanged;

    /// <summary>Sets the toggle to match the real state without counting as a user change.</summary>
    public void SyncToState(TweakState state, bool drifted = false)
    {
        _syncing = true;
        try
        {
            State = state;
            IsOn = state == TweakState.Applied;
            IsChecking = false;
            IsDrifted = drifted;
        }
        finally
        {
            _syncing = false;
        }
        OnPropertyChanged(nameof(IsPending));
        OnPropertyChanged(nameof(PendingText));
        OnPropertyChanged(nameof(StateText));
    }

    public void MarkChecking() => IsChecking = true;

    private static string BuildLockReason(Tweak t, MachineProfile p)
    {
        if (p.Build < t.Requires.MinBuild)
            return t.Requires.MinBuild >= 22000 ? "Needs Windows 11" : $"Needs Windows build {t.Requires.MinBuild} or later";
        return $"Not supported on {p.Edition}. Works on {string.Join(", ", t.Requires.Editions)}";
    }
}

public sealed record SourceLink(string Text, Uri Link);