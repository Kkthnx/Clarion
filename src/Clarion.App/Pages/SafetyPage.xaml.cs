using System.Diagnostics;
using Clarion.App.Services;
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

    protected override void OnNavigatedTo(NavigationEventArgs e) => Rebuild();

    private void Rebuild()
    {
        var all = _app.Runtime.Journal.ReadAll();
        var rows = new List<HistoryRow>();
        foreach (var group in all.GroupBy(x => x.TweakId))
        {
            var item = _app.Items.FirstOrDefault(i => i.Id == group.Key);
            var outstanding = _app.Runtime.Journal.OutstandingFor(group.Key);
            if (outstanding.Count == 0) continue;
            var first = outstanding[0];
            rows.Add(new HistoryRow(
                group.Key,
                item?.Name ?? group.Key,
                $"Applied {first.Time.LocalDateTime:g}",
                string.Join("\n", outstanding.Select(o => o.Operation.Describe())),
                item is { IsApplied: true }));
        }
        History.ItemsSource = rows.OrderByDescending(r => r.When).ToList();
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
        foreach (var id in _app.Items.Where(i => i.IsApplied && _app.Runtime.Journal.OutstandingFor(i.Id).Count > 0).Select(i => i.Id).ToList())
            QueueRevert(id);
    }

    private void QueueRevert(string id)
    {
        var item = _app.Items.FirstOrDefault(i => i.Id == id);
        if (item is { CanToggle: true, IsApplied: true }) item.IsOn = false;
    }

    private void OnOpenRestore(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo("rstrui.exe") { UseShellExecute = true });

    private void OnOpenData(object sender, RoutedEventArgs e)
    {
        Directory.CreateDirectory(EngineFactory.DefaultDataDirectory);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{EngineFactory.DefaultDataDirectory}\"") { UseShellExecute = true });
    }
}

public sealed record HistoryRow(string TweakId, string Name, string When, string Detail, bool CanRevert);
