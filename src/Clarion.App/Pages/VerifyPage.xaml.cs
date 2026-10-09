using Clarion.App.Services;
using Clarion.Core.Drift;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed partial class VerifyPage : Page
{
    private readonly AppServices _app = AppServices.Instance;

    public VerifyPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_app.Drift is null) await ScanAsync(fresh: true);
        else Show(_app.Drift);
    }

    private async void OnScan(object sender, RoutedEventArgs e) => await ScanAsync(fresh: true);

    private async Task ScanAsync(bool fresh)
    {
        ScanButton.IsEnabled = false;
        Busy.IsActive = true;
        SummaryText.Text = "Checking";
        // Each setting is read on its own, some by starting PowerShell, so a long list can take a while. Say where it is.
        var progress = new Progress<(int Done, int Total)>(p =>
            SummaryText.Text = p.Total == 0 ? "Checking" : $"Checking {Math.Min(p.Done + 1, p.Total)} of {p.Total}");
        try
        {
            Show(await _app.VerifyAsync(fresh, progress));
        }
        catch (Exception ex)
        {
            Log.Write($"Verify failed: {ex.Message}");
            SummaryText.Text = "The check could not finish. See the log for details.";
        }
        finally
        {
            Busy.IsActive = false;
            ScanButton.IsEnabled = true;
        }
    }

    private void Show(DriftReport report)
    {
        var returned = report.ReturnedApps.Select(Row).ToList();
        var changed = report.ChangedBack.Select(Row).ToList();
        AppRows.ItemsSource = returned;
        SettingRows.ItemsSource = changed;
        AppsSection.Visibility = returned.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        SettingsSection.Visibility = changed.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        AllGoodText.Visibility = report.HasDrift || report.Checked == 0 ? Visibility.Collapsed : Visibility.Visible;

        SummaryText.Text = report.Checked == 0
            ? "Clarion has not applied anything yet, so there is nothing to check."
            : $"Checked {report.Checked} setting{(report.Checked == 1 ? "" : "s")} at {report.ScannedAt.LocalDateTime:t}.";

        var pending = _app.PendingRestartNow;
        RestartBar.IsOpen = pending is { IsPending: true };
        RestartBar.Visibility = RestartBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (pending is { IsPending: true })
            RestartBar.Message = $"{string.Join(". ", pending.Reasons)}. After it restarts, Clarion will check the {report.Checked} setting{(report.Checked == 1 ? "" : "s")} it applied again, because updates are when changes are most likely to be undone. Scan again then.";

        var stopped = _app.StoppedChecking().Select(x => new StoppedRow(x.Item.Id, x.Item.Name, $"Applied {x.Applied.LocalDateTime:g}")).ToList();
        StoppedRows.ItemsSource = stopped;
        StoppedSection.Visibility = stopped.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        UpdateBar.IsOpen = report.FeatureUpdateSinceLastScan;
        UpdateBar.Visibility = UpdateBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (report.FeatureUpdateSinceLastScan)
            UpdateBar.Message = $"Windows went from build {report.PreviousBuild} to {report.Build} since the last check. Updates are when changes are most likely to be undone, so look at the list below.";

        UnreadableBar.IsOpen = report.Unreadable.Count > 0;
        UnreadableBar.Visibility = UnreadableBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (report.Unreadable.Count > 0)
            UnreadableBar.Message = $"{string.Join(", ", report.Unreadable.Select(NameOf))} could not be checked this time. Scan again in a moment.";
    }

    private string NameOf(string id) => _app.Items.FirstOrDefault(i => i.Id == id)?.Name ?? id;

    private static DriftRow Row(DriftItem item)
    {
        // The exact registry and service steps are for Expert mode. Everyone else gets one plain sentence.
        var steps = string.Join("\n", item.Changed);
        var detail = item.IsReturnedApp || item.Changed.Any(c => c.Contains("installed", StringComparison.OrdinalIgnoreCase)) ? steps
            : AppServices.Instance.ExpertMode ? "No longer in effect. Windows or another program put it back.\n" + steps
            : "No longer in effect. Windows or another program put it back.";
        var when = item.AppliedAt == DateTimeOffset.MinValue ? "" : $"Applied {item.AppliedAt.LocalDateTime:g}";
        return new DriftRow(item.Tweak.Id, item.Tweak.Name, when, detail, item.Tweak.Apply.Any(o => o is Clarion.Core.Model.RemoveAppxPackage) ? "Remove again" : "Put back");
    }

    private void OnFixOne(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string id }) _app.QueueFix([id]);
    }

    private void OnFixApps(object sender, RoutedEventArgs e) =>
        _app.QueueFix(_app.Drift?.ReturnedApps.Select(i => i.Tweak.Id) ?? []);

    private void OnFixSettings(object sender, RoutedEventArgs e) =>
        _app.QueueFix(_app.Drift?.ChangedBack.Select(i => i.Tweak.Id) ?? []);

    private async void OnWatchAgain(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id }) return;
        await _app.WatchAgainAsync(id);
        await ScanAsync(fresh: false);
    }

    private async void OnLeaveOne(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string id }) return;
        await _app.StopTrackingAsync(id);
        if (_app.Drift is { } report) Show(report);
    }
}

public sealed record DriftRow(string TweakId, string Name, string When, string Detail, string FixLabel);

public sealed record StoppedRow(string TweakId, string Name, string When);
