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
        PortableWarning = profile.PortableWarning(tweak) ?? "";
        HoldsBackOnThisPc = profile.HoldsBack(tweak);
        if (InstallsApps && Icon is null)
            _logoFilePath = Clarion.Core.Apps.AppLogoFiles.Default.PathFor(Id, System.IO.Path.Combine(AppContext.BaseDirectory, "Assets"));
    }

    internal Tweak Tweak { get; }
    public TooltipContent Tooltip { get; }
    public bool IsSupported { get; }
    public string LockReason { get; }

    /// <summary>The note for a laptop, empty on a desktop. Shown in the details so the person knows before turning it on.</summary>
    public string PortableWarning { get; }
    public bool HasPortableWarning => PortableWarning.Length > 0;

    /// <summary>True on a laptop for a setting that presets leave out, so a preset does not queue it behind the person's back.</summary>
    public bool HoldsBackOnThisPc { get; }

    /// <summary>False for apps, where every row would say the same "Optional" and that tells nothing.</summary>
    public bool ShowRecommendation => !InstallsApps;

    public bool IsApp => InstallsApps;

    private string? _installedIconPath;

    /// <summary>The icon file of the installed app, or null when it is not installed or Windows lists no icon for it.</summary>
    public string? InstalledIconPath
    {
        get => _installedIconPath;
        private set => SetProperty(ref _installedIconPath, value);
    }

    /// <summary>The logo for an app, or null for an app without one and for every other kind of setting.</summary>
    public Clarion.Core.Apps.AppIcon? Icon => InstallsApps ? Clarion.Core.Apps.AppIcons.Default.For(Id) : null;

    private string? _logoFilePath;

    /// <summary>
    /// The logo picture saved inside the program for an app that has no outline, or null. It is a plain file beside the program, found once when
    /// the row is made, so showing it never touches the network.
    /// </summary>
    public string? LogoFilePath
    {
        get => _logoFilePath;
        private set => SetProperty(ref _logoFilePath, value);
    }

    public string RecommendationText => Tooltip.RecommendationLabel;
    public string RiskText => DetailHelpers.RiskText(Tweak.RiskLevel);
    public string RecommendationHint => DetailHelpers.RecommendationHint(Tweak.Recommendation);
    public string RiskHint => DetailHelpers.RiskHint(Tweak.RiskLevel);

    /// <summary>False for the lowest risk, which every setting on the page says and so tells nothing in a one line row.</summary>
    public bool RiskIsNotable => Tweak.RiskLevel > RiskLevel.Safe;

    /// <summary>The recommendation in one or two words, for the compact list where the full label would crowd out the name.</summary>
    public string RecommendationShort => Tweak.Recommendation switch
    {
        Recommendation.Recommended => "Suggested",
        Recommendation.OnlyIf => "If it fits",
        Recommendation.Avoid => "Avoid",
        _ => "Optional",
    };
    public string EvidenceText => Tooltip.EvidenceLabel;
    public IReadOnlyList<string> ExactChanges => EffectiveTweak.Apply.Select(o => o.Describe()).ToList();
    public IReadOnlyList<SourceLink> SourceLinks => DetailHelpers.Links(Tweak.Sources);
    public IReadOnlyList<string> Provenance => Tweak.Provenance;
    public bool HasSources => Tweak.Sources.Count > 0;
    public bool HasLastError => LastError.Length > 0;

    public ChipKind RecommendationKind => DetailHelpers.RecommendationKind(Tweak.Recommendation);
    public ChipKind RiskKind => DetailHelpers.RiskKind(Tweak.RiskLevel);

    private string _imageNote = "";

    /// <summary>A note about this setting on a customized Windows image, or empty.</summary>
    public string ImageNote
    {
        get => _imageNote;
        set { if (SetProperty(ref _imageNote, value)) OnPropertyChanged(nameof(HasImageNote)); }
    }

    public bool HasImageNote => ImageNote.Length > 0;

    /// <summary>Expert option for app removals: also stop Windows installing the app for accounts created later.</summary>
    public bool CanDeprovision => IsSupported && DeprovisionVariant.CanApply(Tweak);

    public bool DeprovisionToo { get; private set; }

    /// <summary>True once the journal shows this was applied with the option, which Revert cannot undo.</summary>
    public bool DeprovisionLocked { get; private set; }

    /// <summary>True for an app winget can install for the account that runs .Clarion only, which the review offers as a choice.</summary>
    public bool SupportsUserScope => InstallsApps && IsSupported && Clarion.Core.Apps.UserScopeVariant.CanApply(Tweak, Clarion.Core.Apps.AppScopes.Default);

    /// <summary>True when the person chose, in the review, to install this app for their own account only. It is a choice for one run.</summary>
    public bool UserScope { get; private set; }

    public void SetUserScope(bool on)
    {
        on &= SupportsUserScope;
        if (UserScope == on) return;
        UserScope = on;
        OnPropertyChanged(nameof(UserScope));
        OnPropertyChanged(nameof(ExactChanges));
    }

    /// <summary>What actually runs: the catalog entry, or its variant when an option is on.</summary>
    internal Tweak EffectiveTweak
    {
        get
        {
            var tweak = DeprovisionToo ? DeprovisionVariant.Of(Tweak) : Tweak;
            return UserScope ? Clarion.Core.Apps.UserScopeVariant.Of(tweak) : tweak;
        }
    }

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
            // An installed app shows the icon Windows itself lists for it, found by the code its winget record names.
            if (InstallsApps)
                InstalledIconPath = value == TweakState.Applied ? Clarion.Engine.WindowsInstalledIcons.FindIconFile(Clarion.Core.Apps.AppProducts.Default.For(Id)) : null;
            OnPropertyChanged(nameof(CanToggle));
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(StateText));
            OnPropertyChanged(nameof(IsApplied));
            OnPropertyChanged(nameof(IsUnavailable));
            OnPropertyChanged(nameof(RowOpacity));
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
    public bool ReadsQuickly => Tweak.Apply.All(o => o is SetRegistryValue or DeleteRegistryValue or SetRegistryListEntry);

    public bool IsApplied => State == TweakState.Applied;
    public bool IsUnavailable => State == TweakState.Unavailable;

    /// <summary>True when this setting only removes apps, so "nothing to change" means the app is not installed.</summary>
    public bool RemovesApps => Tweak.Apply.Count > 0 && Tweak.Apply.All(o => o is RemoveAppxPackage);

    /// <summary>True when this setting installs an app with winget, so on means installed and off means not installed.</summary>
    public bool InstallsApps => Tweak.Apply.Count > 0 && Tweak.Apply.All(o => o is InstallWingetPackage);

    /// <summary>Rows that do not apply on this PC are dimmed, so they read as inactive and not as a switch that is simply off.</summary>
    public double RowOpacity => !IsSupported || IsUnavailable ? 0.6 : 1.0;
    public bool CanToggle => IsSupported && !IsChecking && State != TweakState.Unavailable;
    public bool IsPending => CanToggle && IsOn != IsApplied;

    /// <summary>
    /// What a screen reader says for the row. A list row is named from its item's text, which would otherwise be the type name.
    /// </summary>
    public override string ToString() => $"{Name}, {StateText}";

    public string StateText => !IsSupported ? "Not on this PC"
        : IsChecking ? "Checking"
        : IsDrifted ? (InstallsApps ? "Removed since" : "Changed back by Windows")
        : State switch
        {
            TweakState.Applied => InstallsApps ? "Installed" : "On",
            TweakState.Partial => "Partly on",
            TweakState.Unavailable => InstallsApps ? "winget not found" : RemovesApps ? "Not installed" : "Nothing to change",
            _ => InstallsApps ? "Not installed" : "Off",
        };

    /// <summary>What happens when the pending change is run.</summary>
    public string PendingText => IsPending
        ? (InstallsApps ? (IsOn ? "Will be installed" : "Will be uninstalled") : IsOn ? "Will be turned on" : "Will be reverted")
        : "";


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

    private static string BuildLockReason(Tweak t, MachineProfile p) => p.WhyNotSupported(t.Requires) ?? "";
}

public sealed record SourceLink(string Text, Uri Link);
