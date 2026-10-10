using System.Diagnostics;
using Clarion.App.Services;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed partial class SafetyPage : Page
{
    private readonly AppServices _app = AppServices.Instance;

    public SafetyPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Rebuild();
        _ = LoadRestorePointsAsync();
    }

    /// <summary>The newest restore points Windows has, so it is clear what is there to go back to.</summary>
    private async Task LoadRestorePointsAsync()
    {
        RestoreStatus.Text = "Reading restore points";
        try
        {
            var list = await Task.Run(() => _app.Runtime.RestorePoints.List(6));
            RestoreList.ItemsSource = list.Select(p => new RestoreRow(p.Description, p.Created.LocalDateTime.ToString("g"))).ToList();
            RestoreStatus.Text = list.Count == 0 ? "Windows has no restore points. System Protection may be off for the system drive." : "";
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or System.ComponentModel.Win32Exception or System.Text.Json.JsonException)
        {
            Log.Write($"Restore points could not be listed: {ex.Message}");
            RestoreList.ItemsSource = null;
            RestoreStatus.Text = "The list of restore points could not be read. Reading it needs administrator rights.";
        }
    }

    private async void OnMakeRestore(object sender, RoutedEventArgs e)
    {
        MakeRestore.IsEnabled = false;
        RestoreStatus.Text = "Making a restore point. This can take a little while.";
        var made = await Task.Run(() => _app.Runtime.RestorePoints.CreateEnsuring($"Clarion {DateTime.Now:yyyy-MM-dd HH:mm} (made by hand)", turnOnProtection: false, Log.Write));
        MakeRestore.IsEnabled = true;
        if (made.Success) await LoadRestorePointsAsync();
        else RestoreStatus.Text = $"{made.Error} If System Protection is off, turn it on for the system drive in System Restore, then try again.";
    }

    private void Rebuild()
    {
        // One read of the journal for every row. Looking each setting up on its own read the whole file once per setting.
        var rows = new List<HistoryRow>();
        foreach (var (id, outstanding) in _app.Runtime.Journal.OutstandingByTweak())
        {
            var item = _app.Items.FirstOrDefault(i => i.Id == id);
            var first = outstanding[0];
            rows.Add(new HistoryRow(
                id,
                item?.Name ?? id,
                $"Applied {first.Time.LocalDateTime:g}",
                string.Join("\n", outstanding.Select(o => o.Operation.Describe())),
                item is { IsApplied: true }) { AppliedAt = first.Time });
        }
        // Newest first by the real time. Sorting the text "Applied 10/9/2026" is alphabetical, not chronological.
        History.ItemsSource = rows.OrderByDescending(r => r.AppliedAt).ToList();
        EmptyText.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        RevertAll.IsEnabled = rows.Any(r => r.CanRevert);
    }

    private void OnRevertOne(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string id }) QueueRevert(id);
        Rebuild();
    }

    private void OnRevertAll(object sender, RoutedEventArgs e)
    {
        var tracked = _app.Runtime.Journal.OutstandingByTweak();
        foreach (var id in _app.Items.Where(i => i.IsApplied && tracked.ContainsKey(i.Id)).Select(i => i.Id).ToList())
            QueueRevert(id);
    }

    private void OnRevertSince(object sender, RoutedEventArgs e)
    {
        if (SinceDate.Date is not { } picked)
        {
            SinceStatus.Text = "Pick a day first.";
            return;
        }
        var applied = _app.Runtime.Journal.OutstandingByTweak().Select(kv => (kv.Key, kv.Value[0].Time));
        var ids = HistoryRange.AppliedAfter(applied, picked.LocalDateTime);
        var queued = 0;
        foreach (var id in ids)
        {
            var item = _app.Items.FirstOrDefault(i => i.Id == id);
            if (item is not { CanToggle: true, IsApplied: true }) continue;
            item.IsOn = false;
            queued++;
        }
        SinceStatus.Text = queued == 0
            ? $"Nothing was applied after {picked.LocalDateTime:MMM d}."
            : $"{queued} setting{(queued == 1 ? "" : "s")} queued. Review them at the bottom.";
        Rebuild();
    }

    private void QueueRevert(string id)
    {
        var item = _app.Items.FirstOrDefault(i => i.Id == id);
        if (item is { CanToggle: true, IsApplied: true }) item.IsOn = false;
    }

    private void OnOpenRestore(object sender, RoutedEventArgs e) =>
        Shell.Open("rstrui.exe");

    private void OnOpenData(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(EngineFactory.DefaultDataDirectory);
        Shell.Open(EngineFactory.DefaultDataDirectory);
    }
}

public sealed record RestoreRow(string Description, string When);

public sealed record HistoryRow(string TweakId, string Name, string When, string Detail, bool CanRevert)
{
    public DateTimeOffset AppliedAt { get; init; }
}
