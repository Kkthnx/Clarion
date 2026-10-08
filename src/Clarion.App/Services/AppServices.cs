using System.Collections.ObjectModel;
using System.Diagnostics;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Model;
using Clarion.Engine;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Clarion.App.Services;

/// <summary>Holds the catalog, the engine and the queue of pending changes for the whole app.</summary>
public sealed partial class AppServices : ObservableObject
{
    public static AppServices Instance { get; } = new();

    private AppServices()
    {
        Runtime = EngineFactory.Create();
        Profile = Runtime.Profile;
        var tweaks = CatalogLoader.LoadEmbedded();
        Presets = CatalogLoader.LoadPresets();
        foreach (var t in tweaks)
        {
            var item = new TweakItem(t, Profile);
            item.PendingChanged += (_, _) => RefreshPending();
            Items.Add(item);
        }
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
                OnPropertyChanged(nameof(SimpleMode));
                ExpertModeChangedByUser?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
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
    public async Task RefreshStatesAsync()
    {
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

        if (slow.Count > 0)
        {
            // Windows features go through DISM, which is much slower than apps, services and tasks.
            // Each group fills in as soon as it is ready instead of waiting for the slowest one.
            var features = slow.Where(i => i.Tweak.Apply.Any(o => o is SetWindowsFeature or SetWindowsCapability)).ToList();
            var others = slow.Except(features).ToList();
            await Task.WhenAll(ReadGroupAsync(features), ReadGroupAsync(others));
        }

        AppliedCount = Items.Count(i => i.IsApplied);
        RefreshPending();
        Log.Write($"State read in {started.ElapsedMilliseconds} ms for {Items.Count} settings");
        Status = IsElevated ? "Ready" : "Not running as administrator. Machine wide changes are locked.";
        IsBusy = false;
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
            try { map[item.Id] = Runtime.Engine.Detect(item.Tweak); }
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

    public int QueuePreset(string name)
    {
        if (!Presets.TryGetValue(name, out var ids)) return 0;
        var count = 0;
        foreach (var item in Items.Where(i => ids.Contains(i.Id, StringComparer.OrdinalIgnoreCase)))
        {
            if (!item.CanToggle || item.IsApplied) continue;
            item.IsOn = true;
            count++;
        }
        return count;
    }

    public async Task<BatchResult> ApplyPendingAsync(bool restorePoint, Action<string> log)
    {
        IsBusy = true;
        var toApply = Pending.Where(p => p.IsOn).Select(p => p.Tweak).ToList();
        var toRevert = Pending.Where(p => !p.IsOn).Select(p => p.Tweak).ToList();
        var options = new BatchOptions { CreateRestorePoint = restorePoint, ContinueWithoutRestorePoint = !restorePoint };

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
