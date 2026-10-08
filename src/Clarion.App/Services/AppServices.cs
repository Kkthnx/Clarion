using System.Collections.ObjectModel;
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


    /// <summary>Reads the real state of every tweak on a background thread.</summary>
    public async Task RefreshStatesAsync()
    {
        IsBusy = true;
        var states = await Task.Run(() =>
        {
            var map = new Dictionary<string, TweakState>();
            foreach (var item in Items)
            {
                if (!item.IsSupported) { map[item.Id] = TweakState.Unavailable; continue; }
                try { map[item.Id] = Runtime.Engine.Detect(item.Tweak); }
                catch (Exception) { map[item.Id] = TweakState.Unavailable; }
            }
            return map;
        });

        foreach (var item in Items) item.SyncToState(states[item.Id]);
        AppliedCount = Items.Count(i => i.IsApplied);
        RefreshPending();
        Status = IsElevated ? "Ready" : "Not running as administrator. Machine wide changes are locked.";
        IsBusy = false;
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

        var result = await Task.Run(() => Runtime.Runner.Execute(toApply, toRevert, Profile, options, log));
        await RefreshStatesAsync();

        foreach (var entry in result.Items)
        {
            var item = Items.FirstOrDefault(i => i.Id == entry.TweakId);
            if (item is not null) item.LastError = entry.Result.Success ? "" : entry.Result.Error ?? "Failed";
        }
        return result;
    }
}
