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

    /// <summary>The monthly check is on but starts a different copy of Clarion, for example one that was moved. Home offers to fix it.</summary>
    public bool MonthlyCheckMoved { get; private set; }

    /// <summary>Looks once at start up. It only reads Task Scheduler, and only when the person turned the check on.</summary>
    public async Task CheckMonthlyTaskAsync()
    {
        if (!Settings.MonthlyVerify || Environment.ProcessPath is not { } exe) return;
        try { MonthlyCheckMoved = await Task.Run(() => MonthlySchedule.PointsElsewhere(exe)); }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or System.ComponentModel.Win32Exception) { Log.Write($"Monthly check location: {ex.Message}"); return; }
        if (MonthlyCheckMoved) Log.Write("The monthly check starts a different copy of .Clarion");
        OnPropertyChanged(nameof(MonthlyCheckMoved));
    }

    /// <summary>Sets the task up again from this copy. Needs administrator rights, like turning the check on.</summary>
    public async Task<string?> RepairMonthlyCheckAsync()
    {
        try
        {
            var exe = Environment.ProcessPath ?? throw new InvalidOperationException(".Clarion could not find its own program file.");
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            await Task.Run(() => MonthlySchedule.Enable(exe, user, DateTime.Today));
            MonthlyCheckMoved = false;
            OnPropertyChanged(nameof(MonthlyCheckMoved));
            return null;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or IOException)
        {
            Log.Write($"Monthly check repair: {ex.Message}");
            return ex.Message;
        }
    }

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
    /// they only installed. A copy that was moved back to an older version is recorded the same way, with no notice.
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
        foreach (var note in EngineFactory.DataFolderNotes) Log.Write(note);
        Profile = Runtime.Profile;
        _systemReport = new(Runtime.System.Read);
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
    public Clarion.Core.SystemInfo.BootMode BootMode { get; } = Clarion.Engine.WindowsBootMode.Detect();

    /// <summary>What starts with Windows on this PC, read from the Run keys and the Startup folders.</summary>
    public Task<IReadOnlyList<Clarion.Core.Startup.StartupEntry>> ReadStartupAsync() =>
        Task.Run(() => Clarion.Core.Startup.StartupList.Build(new WindowsStartupSource()));

    /// <summary>
    /// Turns a startup entry on or off at once, through the engine, so it is recorded and can be reverted like any other change. Nothing is
    /// deleted. Null when it worked, otherwise the reason in plain words.
    /// </summary>
    public async Task<string?> SetStartupAsync(Clarion.Core.Startup.StartupEntry entry, bool enabled)
    {
        if (entry.IsProtected) return "This is part of Windows Security, so .Clarion leaves it as it is.";
        // The history lives in a folder only administrators can write to, and a change that cannot be recorded is not made.
        if (!IsElevated) return "Changing what starts needs administrator rights, because .Clarion records every change. Use Restart as administrator on Home.";
        if (IsChangingThisPc) return "Wait for the job that is running to finish first.";

        var tweak = Clarion.Core.Startup.StartupTweaks.TurnOff(entry);
        TweakResult result;
        try
        {
            result = await Task.Run(() => enabled ? Runtime.Engine.Revert(tweak, Guid.NewGuid()) : Runtime.Engine.Apply(tweak, Guid.NewGuid()));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Log.Write($"{entry.Id}: {ex.Message}");
            return ex.Message;
        }
        Log.Write($"{entry.Id}: {(enabled ? "turned on" : "turned off")}{(result.Success ? "" : " failed " + result.Error)}");
        if (result.Success) OnPropertyChanged(nameof(DriftCount));
        return result.Success ? null : result.Error ?? "It did not work.";
    }

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

    /// <summary>True while a repair job runs. It is set by the Repair page, so closing the window can be held back.</summary>
    public bool RepairRunning { get; set; }

    /// <summary>True when closing the window now could leave a change half done.</summary>
    public bool IsChangingThisPc => IsBusy || RepairRunning || CleanupService.Instance.IsDeleting;
    public ActivityLog Activity { get; } = new();

    private readonly Clarion.Core.Engine.SharedRead<Clarion.Core.SystemInfo.SystemReport> _systemReport;

    /// <summary>The system report from the last read, or null before the first one finishes.</summary>
    public Clarion.Core.SystemInfo.SystemReport? CachedSystemReport => _systemReport.Latest;

    /// <summary>Pages that ask while a read is running share it, so the probe runs once at startup.</summary>
    public Task<Clarion.Core.SystemInfo.SystemReport> GetSystemReportAsync(bool refresh = false) => _systemReport.GetAsync(refresh);

    public string Status { get => _status; set => SetProperty(ref _status, value); }

    private string? _appsProblem;

    /// <summary>Why the apps cannot be read or installed right now, in a sentence, or null when nothing is wrong.</summary>
    public string? AppsProblem { get => _appsProblem; private set => SetProperty(ref _appsProblem, value); }
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
        var waitingOnFeatures = false;

        // Whatever goes wrong in the middle, the app must not stay on "Reading your system" with its buttons locked.
        try
        {
            var fast = Items.Where(i => i.IsSupported && i.ReadsQuickly).ToList();
            // Apps are read when the Apps page is first opened, not at start-up. The read runs winget, which may contact Microsoft's package
            // source, and Clarion should not start that for someone who never uses apps. Without winget there is nothing to run, so they read now.
            var readApps = _appsWanted || !Runtime.Winget.IsAvailable;
            var slow = Items.Where(i => i.IsSupported && !i.ReadsQuickly && (readApps || !i.InstallsApps)).ToList();
            foreach (var item in Items.Where(i => !i.IsSupported)) item.SyncToState(TweakState.Unavailable);
            foreach (var item in slow) item.MarkChecking();

            var fastStates = await Task.Run(() => DetectAll(fast));
            Apply(fast, fastStates);
            AppliedCount = Items.Count(i => i.IsApplied && !i.InstallsApps);

            _featureRead = _appsRead;
            if (slow.Count > 0)
            {
                // Windows features go through DISM, which takes about 15 seconds and nothing else depends on it, so it
                // does not hold up the rest. Those rows stay on "Checking" and cannot be toggled until DISM answers.
                // Apps are read in the background too, from one winget export that takes several seconds, and at the same time as the features.
                var apps = slow.Where(i => i.Tweak.Apply.Any(o => o is InstallWingetPackage or UninstallWingetPackage)).ToList();
                var features = slow.Where(i => i.Tweak.Apply.Any(o => o is SetWindowsFeature or SetWindowsCapability)).ToList();
                var others = slow.Except(features).Except(apps).ToList();
                // The apps read started here replaces the tracked one, so QueueFix and QueueRevert wait for the read that is really running.
                if (apps.Count > 0) _appsRead = ReadAppsAsync(apps);
                _featureRead = Task.WhenAll(ReadGroupAsync(features), _appsRead);
                await ReadGroupAsync(others);
            }

            AppliedCount = Items.Count(i => i.IsApplied && !i.InstallsApps);
            RefreshPending();
            await VerifyQuietlyAsync();
            waitingOnFeatures = !_featureRead.IsCompleted;
            Log.Write($"State read in {started.ElapsedMilliseconds} ms for {Items.Count} settings{(waitingOnFeatures ? ", slower settings still reading" : "")}");
            Status = !IsElevated ? "Not running as administrator. Machine wide changes are locked."
                : BootMode != Clarion.Core.SystemInfo.BootMode.Normal ? "Safe Mode. Apps, scheduled tasks and Windows features cannot be read here."
                : waitingOnFeatures ? (_appsWanted ? "Ready. Windows features and apps are still being read." : "Ready. Windows features are still being read.") : "Ready";
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            Log.Write($"Reading the system failed: {ex.Message}");
            Status = "Reading your system did not finish. See the log for details.";
        }
        finally
        {
            IsBusy = false;
        }
        StatesRefreshed?.Invoke(this, EventArgs.Empty);
        if (waitingOnFeatures) _ = FinishFeaturesAsync(_featureRead);
    }

    private Task _featureRead = Task.CompletedTask;
    private Task _appsRead = Task.CompletedTask;
    private bool _appsWanted;

    /// <summary>
    /// Reads which apps are installed, the first time it is asked for. The Apps page asks when it opens, and so does loading a setup that
    /// lists apps. Until then the apps show as being checked. It runs winget, which may contact Microsoft's package source.
    /// </summary>
    public Task EnsureAppsReadAsync()
    {
        if (_appsWanted) return _appsRead;
        _appsWanted = true;
        var apps = Items.Where(i => i.IsSupported && i.InstallsApps).ToList();
        if (apps.Count == 0) return _appsRead;
        foreach (var app in apps) app.MarkChecking();
        _appsRead = ReadAppsAsync(apps);
        _featureRead = Task.WhenAll(_featureRead, _appsRead);
        return _appsRead;
    }

    private async Task ReadAppsThenAsync(Action then)
    {
        try
        {
            await EnsureAppsReadAsync();
            then();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // Started with its task thrown away, so a failure would otherwise vanish and the person would see nothing happen.
            Log.Write($"Queueing an app after reading the apps failed: {ex.Message}");
        }
    }

    private async Task ReadAppsAsync(List<TweakItem> apps)
    {
        try
        {
            // winget runs as the account Clarion runs as. When that is not the account that is signed in, the apps would be listed and
            // installed for the other one, and the other one may not have winget at all, so it is not run.
            var account = await Task.Run(Clarion.Engine.AccountCheck.Detect);
            if (account.Differs)
            {
                AppsProblem = $".Clarion was started with {account.ElevatedAs}, but you are signed in as {account.SignedInAs}. Apps would be listed and installed for {account.ElevatedAs}, who may not have winget. Close .Clarion and start it again from your own account to use this page.";
                foreach (var app in apps) app.SyncToState(TweakState.Unavailable);
            }
            else
            {
                AppsProblem = null;
                await ReadGroupAsync(apps);
                // Every app unreadable while winget exists means winget itself did not answer, for example with no connection.
                if (Runtime.Winget.IsAvailable && apps.Count > 0 && apps.All(a => a.IsUnavailable))
                    AppsProblem = "winget did not answer, so .Clarion cannot see which apps are installed. Check the internet connection, then press Re-check. The log has the detail.";
            }
        }
        catch (Exception ex) when (ex is not OutOfMemoryException) { Log.Write($"Apps could not be read: {ex.Message}"); }
        RefreshPending();
        StatesRefreshed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>When the slow features read ends, brings the counts and the status up to date.</summary>
    private async Task FinishFeaturesAsync(Task read)
    {
        try { await read; }
        catch (Exception ex) { Log.Write($"Windows features could not be read: {ex.Message}"); }
        if (!ReferenceEquals(read, _featureRead) || IsBusy) return;
        AppliedCount = Items.Count(i => i.IsApplied && !i.InstallsApps);
        RefreshPending();
        if (IsElevated && BootMode == Clarion.Core.SystemInfo.BootMode.Normal) Status = "Ready";
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
        AppliedCount = Items.Count(i => i.IsApplied && !i.InstallsApps);
    }

    private Dictionary<string, TweakState> DetectAll(IEnumerable<TweakItem> items)
    {
        var map = new Dictionary<string, TweakState>();
        // One cause can fail every setting of a kind, such as winget not answering for 135 apps, so each reason is logged once with a count.
        var failures = new Dictionary<string, (string First, int Count)>();
        foreach (var item in items)
        {
            try { map[item.Id] = Runtime.Engine.Detect(item.EffectiveTweak); }
            catch (Exception ex)
            {
                failures[ex.Message] = failures.TryGetValue(ex.Message, out var seen) ? (seen.First, seen.Count + 1) : (item.Id, 1);
                map[item.Id] = TweakState.Unavailable;
            }
        }
        foreach (var (message, (first, count)) in failures)
            Log.Write(count == 1 ? $"Could not read {first}: {message}" : $"Could not read {count} settings, the first being {first}: {message}");
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
        (_troubleshooter ??= new Troubleshooter(Runtime.Journal)).Suspects(symptom, Items.Select(i => i.Tweak).Concat(StartupTweaksInHistory()).ToList(), since);

    /// <summary>Settings the person told Clarion to stop checking, with when they were applied.</summary>
    public IReadOnlyList<(string Id, string Name, DateTimeOffset Applied)> StoppedChecking() =>
        Runtime.Journal.ReleasedByTweak()
            .Select(kv => (Tweak: TweakFor(kv.Key), Applied: kv.Value[0].Time))
            .Where(x => x.Tweak is not null)
            .Select(x => (x.Tweak!.Id, x.Tweak.Name, x.Applied))
            .OrderByDescending(x => x.Applied)
            .ToList();

    /// <summary>The change for an id: a catalog setting, or a startup change worked out from the id itself. Null for anything else.</summary>
    public Tweak? TweakFor(string id) =>
        Items.FirstOrDefault(i => i.Id == id)?.Tweak
        ?? (Clarion.Core.Startup.StartupTweaks.EntryFromId(id) is { } entry ? Clarion.Core.Startup.StartupTweaks.TurnOff(entry) : null);

    /// <summary>The startup changes .Clarion made that are still on, so Verify can check that they hold.</summary>
    private IEnumerable<Tweak> StartupTweaksInHistory() =>
        Clarion.Core.Startup.StartupTweaks.ForIds(Runtime.Journal.OutstandingByTweak().Keys);

    /// <summary>Turns off again the startup entries that came back on. Null when it worked, otherwise the first reason.</summary>
    public async Task<string?> FixStartupAsync(IEnumerable<string> ids)
    {
        foreach (var entry in ids.Select(Clarion.Core.Startup.StartupTweaks.EntryFromId).OfType<Clarion.Core.Startup.StartupEntry>())
        {
            var problem = await SetStartupAsync(entry, enabled: false);
            if (problem is not null) return problem;
        }
        return null;
    }

    /// <summary>Starts checking a setting again. Windows is not changed.</summary>
    public async Task WatchAgainAsync(string id)
    {
        var tweak = TweakFor(id);
        if (tweak is null) return;
        await Task.Run(() => Runtime.Engine.Resume(tweak, Guid.NewGuid()));
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

    /// <summary>Asks the main window to show the settings that match a search, the same as typing it into the search box.</summary>
    public event EventHandler<string>? SearchRequested;

    public void RequestSearch(string query) => SearchRequested?.Invoke(this, query);

    /// <summary>Shows the review dialog for whatever is queued.</summary>
    public void RequestReview()
    {
        if (PendingCount > 0) ReviewRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Compares every setting Clarion applied with what Windows looks like now.</summary>
    public async Task<DriftReport> VerifyAsync(bool fresh, IProgress<(int Done, int Total)>? progress = null)
    {
        var previous = Drift;
        // A startup change is not in the catalog, so the ones in the history are added here to be checked like any other.
        var tweaks = Items.Select(i => i.Tweak).Concat(StartupTweaksInHistory()).ToList();
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
        // An app cannot be queued until the apps have been read, so that is done first and the queueing follows.
        if ((!_appsWanted || !_appsRead.IsCompleted) && Items.Any(i => i.InstallsApps && wanted.Contains(i.Id)))
        {
            _ = ReadAppsThenAsync(() => QueueFix(wanted));
            return;
        }
        foreach (var item in Items.Where(i => wanted.Contains(i.Id) && i.CanToggle && !i.IsApplied)) item.IsOn = true;
        if (PendingCount > 0) ReviewRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Queues the revert of one applied setting, then opens the review dialog.</summary>
    public void QueueRevert(string id)
    {
        var item = Items.FirstOrDefault(i => i.Id == id);
        if (item is { InstallsApps: true } && (!_appsWanted || !_appsRead.IsCompleted))
        {
            _ = ReadAppsThenAsync(() => QueueRevert(id));
            return;
        }
        if (item is not { CanToggle: true, IsApplied: true }) return;
        item.IsOn = false;
        ReviewRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Accepts a change made outside Clarion. Windows is left as it is and the setting is no longer checked.</summary>
    public async Task StopTrackingAsync(string id)
    {
        var tweak = TweakFor(id);
        if (tweak is null) return;
        await Task.Run(() => Runtime.Engine.Release(tweak, Guid.NewGuid()));
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

    /// <summary>How Windows Update looks on this PC, or null before the system has been read.</summary>
    public Clarion.Core.SystemInfo.UpdateVerdict? UpdateVerdict { get; private set; }

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
            UpdateVerdict = report.Updates;
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

    /// <summary>Raised after an unexpected error, so the window can tell the person what was done.</summary>
    public event EventHandler? SafeStateEntered;

    /// <summary>
    /// What follows an error nothing caught while idle: the queue is emptied and every setting is read again from Windows, so nothing
    /// that was queued or shown before the error is acted on.
    /// </summary>
    public void EnterSafeState()
    {
        ClearPending();
        Runtime.Engine.Invalidate();
        _ = RefreshStatesAsync(fresh: true);
        SafeStateEntered?.Invoke(this, EventArgs.Empty);
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

    /// <summary>Like <see cref="QueueIds"/>, but first reads which apps are installed when the list has any, since that is not known until then.</summary>
    public async Task<(int Queued, IReadOnlyList<string> Unknown, int NotAvailable)> QueueIdsAsync(IEnumerable<string> ids)
    {
        var list = ids.ToList();
        if (Items.Any(i => i.InstallsApps && list.Contains(i.Id, StringComparer.OrdinalIgnoreCase))) await EnsureAppsReadAsync();
        return QueueIds(list);
    }

    private readonly Clarion.Core.Profiles.QueueCarry _carry = new(Clarion.Core.Profiles.QueueCarry.DefaultPath);

    /// <summary>Writes the queue down before the restart that asks for administrator rights, so the new window can queue it again.</summary>
    public void CarryQueue() => _carry.Save(Pending.Where(p => p.IsOn).Select(p => p.Id));

    /// <summary>The restart did not happen, so the written queue is not needed.</summary>
    public void DiscardCarriedQueue() => _carry.Discard();

    /// <summary>
    /// Queues what was carried across the restart and opens the review again. Only a copy with administrator rights does it, since only
    /// that copy can apply anything. True when a queue came back, which also means the person has already made their first choice.
    /// </summary>
    public async Task<bool> RestoreCarriedQueueAsync()
    {
        if (!IsElevated) return false;
        var ids = _carry.Take();
        if (ids.Count == 0) return false;
        var (queued, _, _) = await QueueIdsAsync(ids);
        if (queued == 0) return false;
        UpdateSettings(s => s with { FirstRunDone = true });
        Status = $"{queued} setting{(queued == 1 ? "" : "s")} from before the restart {(queued == 1 ? "is" : "are")} queued again.";
        ReviewRequested?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public IReadOnlyList<string> AppliedIds() => Items.Where(i => i.IsApplied && i.IsSupported).Select(i => i.Id).ToList();

    public async Task<BatchResult> ApplyPendingAsync(bool restorePoint, Action<string> log, Action<BatchStep>? step = null, bool turnOnProtection = false, CancellationToken stop = default)
    {
        IsBusy = true;
        var toApply = Pending.Where(p => p.IsOn).Select(p => p.EffectiveTweak).ToList();
        // The new choice replaces the old one in an exclusive group, so the old one must not be reverted afterward.
        var replacedGroups = toApply.Select(t => t.ExclusiveGroup).Where(g => g is not null).ToHashSet();
        var toRevert = Pending.Where(p => !p.IsOn && !(p.Tweak.ExclusiveGroup is not null && replacedGroups.Contains(p.Tweak.ExclusiveGroup)))
            .Select(p => p.Tweak).ToList();
        var options = new BatchOptions { CreateRestorePoint = restorePoint, ContinueWithoutRestorePoint = !restorePoint, Step = step, TurnOnSystemProtection = turnOnProtection, Stop = stop };

        Action<string> both = msg => { Log.Write(msg); log(msg); };
        Log.Write($"Run started: {toApply.Count} to apply, {toRevert.Count} to revert, restore point {(restorePoint ? "on" : "off")}");
        BatchResult result;
        try
        {
            result = await Task.Run(() => Runtime.Runner.Execute(toApply, toRevert, Profile, options, both));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // Some changes may already be made, so the real state is read again, and the app must not stay busy for good.
            Log.Write($"Run failed: {ex.Message}");
            IsBusy = false;
            await RefreshStatesAsync();
            throw;
        }
        foreach (var entry in result.Items)
            Log.Write($"{entry.TweakId}: {(entry.Result.Success ? (entry.Skipped ? "skipped" : "ok") : "failed " + entry.Result.Error)}");
        if (result.Blocked is not null) Log.Write($"Blocked: {result.Blocked}");
        if (result.Stopped) Log.Write($"Stopped by the person: {result.NotStarted} not started");
        await RefreshStatesAsync();

        foreach (var entry in result.Items)
        {
            var item = Items.FirstOrDefault(i => i.Id == entry.TweakId);
            if (item is not null) item.LastError = entry.Result.Success ? "" : entry.Result.Error ?? "Failed";
        }
        return result;
    }
}
