using System.Collections.ObjectModel;
using System.Diagnostics;
using Clarion.Core.Appx;
using Clarion.Core.Catalog;
using Clarion.Core.Drift;
using Clarion.Core.Settings;
using Clarion.Core.Scheduling;
using Clarion.Core.Updates;
using Clarion.Core.SystemInfo;
using Clarion.Core.Troubleshoot;
using Clarion.Core.Engine;
using Clarion.Core.Model;
using Clarion.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clarion.App.Services;

/// <summary>Holds the catalog, the engine and the queue of pending changes for the whole app.</summary>
public sealed partial class AppServices : UiObservableObject
{
    // Declared before Instance on purpose. Static initializers run in order, and the constructor reads Version.
    public static string Version { get; } =
        (System.Reflection.CustomAttributeExtensions.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>(System.Reflection.Assembly.GetEntryAssembly()!)?.InformationalVersion ?? "0.1.0").Split('+')[0];

    public static AppServices Instance { get; } = new();

    private readonly UserSettingsStore _settingsStore = new(Path.Combine(EngineFactory.DefaultDataDirectory, "settings.json"));

    public MonthlyVerifySchedule MonthlySchedule { get; } = new(new WindowsScheduledTaskStore(new WindowsProcessRunner()));

    private readonly ScheduledVerifyStore _scheduledResult = new(Path.Combine(EngineFactory.DefaultDataDirectory, "scheduled-verify.json"));

    /// <summary>What the last scheduled check found, as long as the person has not looked at it yet.</summary>
    public ScheduledVerifyResult? UnseenScheduledResult()
    {
        var r = _scheduledResult.Load();
        return r is { Seen: false, Attention: > 0 } ? r : null;
    }

    public ScheduledVerifyResult? LastScheduledResult() => _scheduledResult.Load();

    public void MarkScheduledSeen() => _scheduledResult.MarkSeen();

    /// <summary>A newer version that is on offer, or null. Clarion only points to the page. It never downloads or installs.</summary>
    public UpdateOffer? UpdateAvailable { get; private set; }

    /// <summary>
    /// Asks GitHub for the list of releases. Called when the person presses the button, or at start up if they turned that on.
    /// Returns one plain sentence for the person to read.
    /// </summary>
    public async Task<string> CheckForUpdateAsync(bool manual)
    {
        try
        {
            // CLARION_RELEASES_JSON names a file to read the release list from instead of GitHub. It is for trying the update screens without
            // publishing anything, and is not something people need.
            var testFile = Environment.GetEnvironmentVariable("CLARION_RELEASES_JSON");
            var json = testFile is { Length: > 0 } && File.Exists(testFile)
                ? await File.ReadAllTextAsync(testFile)
                : await new GitHubReleaseSource(Version).GetReleasesJsonAsync(default);
            var releases = UpdateCheck.ParseReleases(json);
            var offer = UpdateCheck.Newest(Version, releases);
            // Every newer release, not only the latest, so someone who skipped a version can see what they missed.
            NewerReleases = WhatsNew.FromReleases(UpdateCheck.NewerThan(Version, releases));
            OnPropertyChanged(nameof(NewerReleases));
            UpdateSettings(s => s with { LastUpdateCheck = DateTimeOffset.UtcNow });
            // A version the person hid stays hidden for the automatic check. Pressing the button asks again on purpose.
            UpdateAvailable = offer is not null && (manual || UpdateCheck.ShouldShow(offer, Settings.DismissedUpdate)) ? offer : null;
            OnPropertyChanged(nameof(UpdateAvailable));
            return offer is null ? $"You have the newest version ({Version})." : $"{offer.Name} is available.";
        }
        catch (UpdateCheckException ex)
        {
            Log.Write($"Update check: {ex.Message}");
            return ex.Message;
        }
    }

    /// <summary>The automatic check, at most about once a day, and only when the person turned it on.</summary>
    public async Task StartupUpdateCheckAsync()
    {
        var last = Settings.LastUpdateCheck;
        if (!Settings.CheckForUpdates || (last is { } l && DateTimeOffset.UtcNow - l < TimeSpan.FromHours(20))) return;
        await CheckForUpdateAsync(manual: false);
    }

    public void HideUpdate()
    {
        if (UpdateAvailable is not { } offer) return;
        UpdateSettings(s => s with { DismissedUpdate = offer.Tag });
        UpdateAvailable = null;
        NewerReleases = [];
        OnPropertyChanged(nameof(UpdateAvailable));
        OnPropertyChanged(nameof(NewerReleases));
    }

    /// <summary>The releases newer than this one found by the last check, newest first, with what changed in each.</summary>
    public IReadOnlyList<ReleaseNotes> NewerReleases { get; private set; } = [];

    /// <summary>The version this one replaced, when Clarion was updated since the person last looked. Otherwise null.</summary>
    public string? UpdatedFromVersion { get; private set; }

    /// <summary>
    /// Compares the version running now with the one last seen. The first run on a PC just records it, so nobody is told they updated when
    /// they only installed. A copy that was moved back to an older version is recorded quietly too.
    /// </summary>
    private void DetectUpdatedVersion()
    {
        var seen = Settings.LastSeenVersion;
        var now = ReleaseVersion.Parse(Version);
        var before = ReleaseVersion.Parse(seen);

        if (before is null || now is null || now <= before)
        {
            if (seen != Version) UpdateSettings(s => s with { LastSeenVersion = Version });
            return;
        }
        UpdatedFromVersion = seen;
    }

    /// <summary>Records that the person has seen what changed, so the "you updated" notice goes away.</summary>
    public void AcknowledgeUpdated()
    {
        if (UpdatedFromVersion is null) return;
        UpdatedFromVersion = null;
        UpdateSettings(s => s with { LastSeenVersion = Version });
        OnPropertyChanged(nameof(UpdatedFromVersion));
    }

    /// <summary>What Clarion remembers between runs. Change it with <see cref="UpdateSettings"/> so it is saved.</summary>
    public UserSettings Settings { get; private set; }

    public void UpdateSettings(Func<UserSettings, UserSettings> change)
    {
        Settings = change(Settings);
        if (!_settingsStore.Save(Settings)) Log.Write("Settings could not be saved");
    }

    private AppServices()
    {
        Settings = _settingsStore.Load();
        _expertMode = Settings.ExpertMode;
        DetectUpdatedVersion();
        Runtime = EngineFactory.Create();
        Profile = Runtime.Profile;
        var tweaks = CatalogLoader.LoadEmbedded();
        Presets = CatalogLoader.LoadPresets();
        foreach (var t in tweaks)
        {
            var item = new TweakItem(t, Profile);
            item.PendingChanged += OnItemPendingChanged;
            Items.Add(item);
        }
    }

    /// <summary>Choices of one setting, such as a power plan, are exclusive. Picking one clears the other pending picks.</summary>
    private void OnItemPendingChanged(object? sender, EventArgs e)
    {
        if (sender is TweakItem { IsOn: true, IsApplied: false, Tweak.ExclusiveGroup: { } group } picked)
        {
            foreach (var other in Items.Where(i => i != picked && i.Tweak.ExclusiveGroup == group && i.IsOn && !i.IsApplied))
                other.IsOn = false;
        }
        RefreshPending();
    }

    public ClarionRuntime Runtime { get; }
    public MachineProfile Profile { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Presets { get; }
    public ObservableCollection<TweakItem> Items { get; } = [];
    public ObservableCollection<TweakItem> Pending { get; } = [];
    public bool IsElevated => Runtime.Runner.IsElevated;

    private bool _expertMode;
    private bool _isBusy;
    private string _status = "Reading your system";
    private int _appliedCount;

    public bool ExpertMode
    {
        get => _expertMode;
        set
        {
            if (SetProperty(ref _expertMode, value))
            {
                UpdateSettings(s => s with { ExpertMode = value });
                OnPropertyChanged(nameof(SimpleMode));
                ExpertModeChangedByUser?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public ActivityLog Activity { get; } = new();

    private Clarion.Core.SystemInfo.SystemReport? _systemReport;

    /// <summary>The system report from the last read, or null before the first one finishes.</summary>
    public Clarion.Core.SystemInfo.SystemReport? CachedSystemReport => _systemReport;

    public async Task<Clarion.Core.SystemInfo.SystemReport> GetSystemReportAsync(bool refresh = false)
    {
        if (refresh || _systemReport is null) _systemReport = await Task.Run(Runtime.System.Read);
        return _systemReport;
    }

    public string Status { get => _status; set => SetProperty(ref _status, value); }
    public int AppliedCount { get => _appliedCount; set => SetProperty(ref _appliedCount, value); }

    public bool SimpleMode => !ExpertMode;
    public int PendingCount => Pending.Count;

    public event EventHandler? ExpertModeChangedByUser;
    public event EventHandler? StatesRefreshed;


    /// <summary>
    /// Reads the real state of every tweak. Registry based ones come back at once. The ones that need
    /// PowerShell are warmed up in parallel and fill in a moment later.
    /// </summary>
    public async Task RefreshStatesAsync(bool fresh = false)
    {
        // A slow features read from the last refresh may still be running. Let it finish before the caches are cleared.
        if (_featureRead is { IsCompleted: false } pending) await pending;
        if (fresh) Runtime.Engine.Invalidate();
        SyncDeprovisionFlags();
        IsBusy = true;
        Status = "Reading your system";
        var started = Stopwatch.StartNew();

        var fast = Items.Where(i => i.IsSupported && i.ReadsQuickly).ToList();
        var slow = Items.Where(i => i.IsSupported && !i.ReadsQuickly).ToList();
        foreach (var item in Items.Where(i => !i.IsSupported)) item.SyncToState(TweakState.Unavailable);
        foreach (var item in slow) item.MarkChecking();

        var fastStates = await Task.Run(() => DetectAll(fast));
        Apply(fast, fastStates);
        AppliedCount = Items.Count(i => i.IsApplied);

        _featureRead = Task.CompletedTask;
        if (slow.Count > 0)
        {
            // Windows features go through DISM, which takes about 15 seconds and nothing else depends on it, so it
            // does not hold up the rest. Those rows stay on "Checking" and cannot be toggled until DISM answers.
            var features = slow.Where(i => i.Tweak.Apply.Any(o => o is SetWindowsFeature or SetWindowsCapability)).ToList();
            var others = slow.Except(features).ToList();
            _featureRead = ReadGroupAsync(features);
            await ReadGroupAsync(others);
        }

        AppliedCount = Items.Count(i => i.IsApplied);
        RefreshPending();
        await VerifyQuietlyAsync();
        var waitingOnFeatures = !_featureRead.IsCompleted;
        Log.Write($"State read in {started.ElapsedMilliseconds} ms for {Items.Count} settings{(waitingOnFeatures ? ", Windows features still reading" : "")}");
        Status = !IsElevated ? "Not running as administrator. Machine wide changes are locked."
            : waitingOnFeatures ? "Ready. Windows features are still being read." : "Ready";
        IsBusy = false;
        StatesRefreshed?.Invoke(this, EventArgs.Empty);
        if (waitingOnFeatures) _ = FinishFeaturesAsync(_featureRead);
    }

    private Task _featureRead = Task.CompletedTask;

    /// <summary>When the slow features read ends, brings the counts and the status up to date.</summary>
    private async Task FinishFeaturesAsync(Task read)
    {
        try { await read; }
        catch (Exception ex) { Log.Write($"Windows features could not be read: {ex.Message}"); }
        if (!ReferenceEquals(read, _featureRead) || IsBusy) return;
        AppliedCount = Items.Count(i => i.IsApplied);
        RefreshPending();
        if (IsElevated) Status = "Ready";
        StatesRefreshed?.Invoke(this, EventArgs.Empty);
    }

    private async Task ReadGroupAsync(List<TweakItem> group)
    {
        if (group.Count == 0) return;
        var states = await Task.Run(() =>
        {
            Runtime.Engine.WarmUp(group.Select(i => i.Tweak));
            return DetectAll(group);
        });
        Apply(group, states);
        AppliedCount = Items.Count(i => i.IsApplied);
    }

    private Dictionary<string, TweakState> DetectAll(IEnumerable<TweakItem> items)
    {
        var map = new Dictionary<string, TweakState>();
        foreach (var item in items)
        {
            try { map[item.Id] = Runtime.Engine.Detect(item.EffectiveTweak); }
            catch (Exception ex)
            {
                Log.Write($"Could not read {item.Id}: {ex.Message}");
                map[item.Id] = TweakState.Unavailable;
            }
        }
        return map;
    }

    private void Apply(IEnumerable<TweakItem> items, Dictionary<string, TweakState> states)
    {
        var outstanding = Runtime.Journal.TweakIdsWithOutstanding();
        foreach (var item in items)
        {
            var state = states[item.Id];
            var drifted = state != TweakState.Applied && outstanding.Contains(item.Id);
            item.SyncToState(state, drifted);
        }
    }
    private Troubleshooter? _troubleshooter;

    /// <summary>Settings Clarion changed that could explain a symptom, named ones first, newest first.</summary>
    public IReadOnlyList<Suspect> SuspectsFor(Symptom symptom, DateTimeOffset? since = null) =>
        (_troubleshooter ??= new Troubleshooter(Runtime.Journal)).Suspects(symptom, Items.Select(i => i.Tweak).ToList(), since);

    /// <summary>Settings the person told Clarion to stop checking, with when they were applied.</summary>
    public IReadOnlyList<(TweakItem Item, DateTimeOffset Applied)> StoppedChecking() =>
        Runtime.Journal.ReleasedByTweak()
            .Select(kv => (Item: Items.FirstOrDefault(i => i.Id.Equals(kv.Key, StringComparison.OrdinalIgnoreCase)), Applied: kv.Value[0].Time))
            .Where(x => x.Item is not null)
            .Select(x => (x.Item!, x.Applied))
            .OrderByDescending(x => x.Applied)
            .ToList();

    /// <summary>Starts checking a setting again. Windows is not changed.</summary>
    public async Task WatchAgainAsync(string id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item is null) return;
        await Task.Run(() => Runtime.Engine.Resume(item.Tweak, Guid.NewGuid()));
        Log.Write($"{id}: checking again");
        await RefreshStatesAsync();
    }

    /// <summary>Whether Windows has an update waiting for a restart, as of the last check.</summary>
    public PendingRestartInfo? PendingRestartNow { get; private set; }

    private DriftReport? _drift;

    /// <summary>The latest verify scan, or null before the first one finishes.</summary>
    public DriftReport? Drift
    {
        get => _drift;
        private set
        {
            _drift = value;
            OnPropertyChanged(nameof(Drift));
            OnPropertyChanged(nameof(DriftCount));
        }
    }

    public int DriftCount => Drift?.Items.Count ?? 0;

    /// <summary>Asks the main window to open the page with this navigation tag.</summary>
    public event EventHandler<string>? NavigateRequested;

    /// <summary>Asks the main window to show the review dialog for the queued changes.</summary>
    public event EventHandler? ReviewRequested;

    public void RequestNavigate(string tag) => NavigateRequested?.Invoke(this, tag);

    /// <summary>Shows the review dialog for whatever is queued.</summary>
    public void RequestReview()
    {
        if (PendingCount > 0) ReviewRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Compares every setting Clarion applied with what Windows looks like now.</summary>
    public async Task<DriftReport> VerifyAsync(bool fresh, IProgress<(int Done, int Total)>? progress = null)
    {
        var previous = Drift;
        var tweaks = Items.Select(i => i.Tweak).ToList();
        var report = await Task.Run(() => Runtime.Drift.Scan(tweaks, Profile, fresh, progress));
        PendingRestartNow = Clarion.Core.SystemInfo.PendingRestart.Check(new WindowsPendingRestartProbe());
        // A scan saves the build it saw, so the next one would forget a feature update that already happened this session.
        if (previous is { FeatureUpdateSinceLastScan: true } && !report.FeatureUpdateSinceLastScan)
            report = report with { PreviousBuild = previous.PreviousBuild };
        Drift = report;
        Log.Write($"Verify: {report.Checked} checked, {report.ChangedBack.Count} changed back, {report.ReturnedApps.Count} apps returned, {report.Unreadable.Count} unreadable");
        return report;
    }

    private async Task VerifyQuietlyAsync()
    {
        try { await VerifyAsync(fresh: false); }
        catch (Exception ex) { Log.Write($"Verify failed: {ex.Message}"); }
    }

    /// <summary>Queues the given settings to be put back, then opens the review dialog.</summary>
    public void QueueFix(IEnumerable<string> ids)
    {
        var wanted = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in Items.Where(i => wanted.Contains(i.Id) && i.CanToggle && !i.IsApplied)) item.IsOn = true;
        if (PendingCount > 0) ReviewRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Queues the revert of one applied setting, then opens the review dialog.</summary>
    public void QueueRevert(string id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item is not { CanToggle: true, IsApplied: true }) return;
        item.IsOn = false;
        ReviewRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Accepts a change made outside Clarion. Windows is left as it is and the setting is no longer checked.</summary>
    public async Task StopTrackingAsync(string id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item is null) return;
        await Task.Run(() => Runtime.Engine.Release(item.Tweak, Guid.NewGuid()));
        Log.Write($"{id}: stopped tracking");
        await RefreshStatesAsync();
    }

    /// <summary>Applies the Expert option to an app removal and rereads its state. Ticking it on a removed app queues the extra step.</summary>
    public void SetDeprovision(TweakItem item, bool on)
    {
        if (!item.CanDeprovision || item.DeprovisionLocked) return;
        var wasOn = item.IsOn;
        item.SetDeprovision(on, locked: false);
        TweakState state;
        try { state = Runtime.Engine.Detect(item.EffectiveTweak); }
        catch (Exception ex) { Log.Write($"Could not read {item.Id}: {ex.Message}"); return; }
        item.SyncToState(state);
        if (on && wasOn && state == TweakState.NotApplied && item.CanToggle) item.IsOn = true;
        RefreshPending();
    }

    private void SyncDeprovisionFlags()
    {
        var recorded = Runtime.Journal.OutstandingByTweak();
        foreach (var item in Items.Where(i => i.CanDeprovision))
        {
            var done = recorded.TryGetValue(item.Id, out var entries) && DeprovisionVariant.WasRecorded(entries);
            if (done || item.DeprovisionLocked) item.SetDeprovision(done, done);
        }
    }

    /// <summary>What the install check found, or null before the system has been read.</summary>
    public Clarion.Core.SystemInfo.InstallVerdict? ImageVerdict { get; private set; }

    /// <summary>How many settings have a note about this Windows image.</summary>
    public int ImageNoteCount { get; private set; }

    /// <summary>Reads the system once, then adds a note to each setting that works differently on this image.</summary>
    public async Task LoadImageNotesAsync()
    {
        try
        {
            var report = await GetSystemReportAsync();
            var notes = Clarion.Core.SystemInfo.ImageAdvice.Notes(Items.Select(i => i.Tweak).ToList(), report.Install);
            foreach (var item in Items) item.ImageNote = notes.TryGetValue(item.Id, out var note) ? note : "";
            ImageVerdict = report.Install;
            ImageNoteCount = notes.Count;
            OnPropertyChanged(nameof(ImageVerdict));
        }
        catch (Exception ex)
        {
            Log.Write($"Could not read the image details: {ex.Message}");
        }
    }

    public void RefreshPending()
    {
        Pending.Clear();
        foreach (var item in Items.Where(i => i.IsPending)) Pending.Add(item);
        OnPropertyChanged(nameof(PendingCount));
    }

    public void ClearPending()
    {
        foreach (var item in Items.Where(i => i.IsPending).ToList()) item.IsOn = item.IsApplied;
    }

    public int QueuePreset(string name) => Presets.TryGetValue(name, out var ids) ? QueueIds(ids).Queued : 0;

    /// <summary>Queues the listed settings to be turned on. Returns how many were queued and which names are not in this catalog.</summary>
    public (int Queued, IReadOnlyList<string> Unknown, int NotAvailable) QueueIds(IEnumerable<string> ids)
    {
        var wanted = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var queued = 0;
        var notAvailable = 0;
        foreach (var item in Items.Where(i => wanted.Contains(i.Id)))
        {
            if (item.IsApplied) continue;
            if (!item.CanToggle) { notAvailable++; continue; }
            item.IsOn = true;
            queued++;
        }
        var unknown = wanted.Where(id => Items.All(i => !i.Id.Equals(id, StringComparison.OrdinalIgnoreCase))).Order().ToList();
        return (queued, unknown, notAvailable);
    }

    public IReadOnlyList<string> AppliedIds() => Items.Where(i => i.IsApplied && i.IsSupported).Select(i => i.Id).ToList();

    public async Task<BatchResult> ApplyPendingAsync(bool restorePoint, Action<string> log, Action<BatchStep>? step = null, bool turnOnProtection = false)
    {
        IsBusy = true;
        var toApply = Pending.Where(p => p.IsOn).Select(p => p.EffectiveTweak).ToList();
        // The new choice replaces the old one in an exclusive group, so the old one must not be reverted afterward.
        var replacedGroups = toApply.Select(t => t.ExclusiveGroup).Where(g => g is not null).ToHashSet();
        var toRevert = Pending.Where(p => !p.IsOn && !(p.Tweak.ExclusiveGroup is not null && replacedGroups.Contains(p.Tweak.ExclusiveGroup)))
            .Select(p => p.Tweak).ToList();
        var options = new BatchOptions { CreateRestorePoint = restorePoint, ContinueWithoutRestorePoint = !restorePoint, Step = step, TurnOnSystemProtection = turnOnProtection };

        Action<string> both = msg => { Log.Write(msg); log(msg); };
        Log.Write($"Run started: {toApply.Count} to apply, {toRevert.Count} to revert, restore point {(restorePoint ? "on" : "off")}");
        var result = await Task.Run(() => Runtime.Runner.Execute(toApply, toRevert, Profile, options, both));
        foreach (var entry in result.Items)
            Log.Write($"{entry.TweakId}: {(entry.Result.Success ? (entry.Skipped ? "skipped" : "ok") : "failed " + entry.Result.Error)}");
        if (result.Blocked is not null) Log.Write($"Blocked: {result.Blocked}");
        await RefreshStatesAsync();

        foreach (var entry in result.Items)
        {
            var item = Items.FirstOrDefault(i => i.Id == entry.TweakId);
            if (item is not null) item.LastError = entry.Result.Success ? "" : entry.Result.Error ?? "Failed";
        }
        return result;
    }
}
