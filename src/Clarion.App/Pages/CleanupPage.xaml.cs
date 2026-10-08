using Clarion.Core.Model;
using Clarion.Core.Actions;
using Clarion.App.Controls;
using Clarion.App.Services;
using Clarion.Core.Cleanup;
using Clarion.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed record CleanupGroup(string Name, IReadOnlyList<CleanupItem> Items);

public sealed partial class CleanupPage : Page
{
    private readonly CleanupService _svc = CleanupService.Instance;
    private bool _timerRunning;

    public CleanupPage()
    {
        InitializeComponent();
        Groups.ItemsSource = _svc.Items.GroupBy(i => i.Group).Select(g => new CleanupGroup(g.Key, g.ToList())).ToList();
        foreach (var i in _svc.Items) i.CheckedChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateSelected);
        _svc.PropertyChanged += (_, _) => DispatcherQueue.TryEnqueue(Refresh);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Refresh();
        var note = _svc.CheckQueuedFromLastTime();
        if (note is not null)
        {
            QueueBar.Message = note;
            QueueBar.IsOpen = true;
        }
    }

    private void Refresh()
    {
        StatusLabel.Text = _svc.Status;
        var busy = _svc.IsBusy;
        ScanButton.IsEnabled = !busy;
        SuggestButton.IsEnabled = !busy;
        CleanButton.IsEnabled = !busy;
        StopButton.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        ProgressPanel.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        LiveText.Text = $"{ByteSize.Format(_svc.FreedLive)} freed so far, {_svc.FilesLive:N0} files";
        FileText.Text = _svc.CurrentFile;
        UpdateSelected();
    }

    private void UpdateSelected()
    {
        var chosen = _svc.Items.Count(i => i.IsChecked && i.CanSelect);
        SelectedText.Text = chosen == 0 ? "Nothing selected" : $"{chosen} selected, {ByteSize.Format(_svc.SelectedBytes)}";
    }

    private async void OnScan(object sender, RoutedEventArgs e)
    {
        ResultBar.IsOpen = false;
        RestartRow.Visibility = Visibility.Collapsed;
        await _svc.ScanAsync();
    }

    private void OnSelectSuggested(object sender, RoutedEventArgs e) => _svc.SelectSuggested();

    private void OnStop(object sender, RoutedEventArgs e) => _svc.Cancel();

    private async void OnClean(object sender, RoutedEventArgs e)
    {
        var chosen = _svc.Items.Where(i => i.IsChecked && i.CanSelect).ToList();
        if (chosen.Count == 0)
        {
            ResultBar.Severity = InfoBarSeverity.Warning;
            ResultBar.Title = "Nothing selected";
            ResultBar.Message = "Tick the rows you want to clean, or press Select suggested.";
            ResultBar.IsOpen = true;
            return;
        }

        var preview = PreviewSwitch.IsOn;
        if (!preview)
        {
            var irreversible = chosen.Where(i => i.Target.Irreversible).ToList();
            var body = new StackPanel { Spacing = 8 };
            body.Children.Add(new TextBlock { Text = $"{chosen.Count} rows will be cleaned.", TextWrapping = TextWrapping.Wrap });
            if (irreversible.Count > 0)
            {
                body.Children.Add(new TextBlock
                {
                    Text = $"These cannot be undone: {string.Join(", ", irreversible.Select(i => i.Name))}.",
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                });
            }
            body.Children.Add(new TextBlock { Text = "Only files that are rebuilt on demand are removed. Files in use are left alone.", TextWrapping = TextWrapping.Wrap, Opacity = 0.8 });

            var dialog = new ContentDialog
            {
                Title = "Clean now?",
                Content = body,
                PrimaryButtonText = "Clean",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot,
            };
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        }

        ResultBar.IsOpen = false;
        _ = PumpAsync();
        var summary = await _svc.CleanAsync(preview, queueDriverFiles: true);
        ShowSummary(summary, chosen);
    }

    /// <summary>Keeps the live counters moving while a clean runs.</summary>
    private async Task PumpAsync()
    {
        if (_timerRunning) return;
        _timerRunning = true;
        while (_svc.IsBusy)
        {
            Refresh();
            await Task.Delay(150);
        }
        _timerRunning = false;
        Refresh();
    }

    private void ShowSummary(CleanupSummary s, List<CleanupItem> rows)
    {
        ResultBar.Severity = s.Cancelled ? InfoBarSeverity.Warning : InfoBarSeverity.Success;
        ResultBar.Title = s.Cancelled ? "Stopped" : s.Preview ? "Preview finished" : "Done";
        var text = $"{(s.Preview ? "Would free" : "Freed")} {ByteSize.Format(s.BytesFreed)} from {s.FilesDeleted:N0} files in {s.Duration.TotalSeconds:0.0} seconds.";
        if (s.FilesLocked > 0) text += $" {s.FilesLocked:N0} files were in use and left alone.";
        if (s.Queued > 0) text += $" {s.Queued:N0} files will be removed at the next restart.";
        ResultBar.Message = text;
        ResultBar.IsOpen = true;
        RestartRow.Visibility = s.Queued > 0 || !s.Preview ? Visibility.Visible : Visibility.Collapsed;
        RestartButton.Visibility = s.Queued > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void OnRestart(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Restart to finish?",
            Content = "Save your work first. Windows counts down 60 seconds. An app with unsaved changes will make Windows stop and ask, and you can cancel the countdown here.",
            PrimaryButtonText = "Restart in 60 seconds",
            CloseButtonText = "Not now",
            XamlRoot = XamlRoot,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        try
        {
            RestartHelper.Schedule(60, "Clarion is restarting Windows to finish cleaning. Save your work.");
            CancelRestartButton.Visibility = Visibility.Visible;
            RestartButton.IsEnabled = false;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            ResultBar.Severity = InfoBarSeverity.Error;
            ResultBar.Title = "Could not start the restart";
            ResultBar.Message = ex.Message;
            ResultBar.IsOpen = true;
        }
    }

    private void OnCancelRestart(object sender, RoutedEventArgs e)
    {
        RestartHelper.Cancel();
        CancelRestartButton.Visibility = Visibility.Collapsed;
        RestartButton.IsEnabled = true;
    }

    private void OnCopyReport(object sender, RoutedEventArgs e)
    {
        var text = _svc.LastSummary is { } s ? _svc.BuildReport(_svc.Items, s) : _svc.LastReportText();
        if (text.Length == 0) return;
        var package = new Windows.ApplicationModel.DataTransfer.DataPackage();
        package.SetText(text);
        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(package);
    }

    private void OnCardTapped(object sender, TappedRoutedEventArgs e)
    {
        if (e.OriginalSource is Microsoft.UI.Xaml.Controls.Primitives.ToggleButton) return;
        if ((sender as FrameworkElement)?.Tag is not CleanupItem item) return;
        Pane.Show(item, technical: true);
    }

    private void OnTipOpened(object sender, RoutedEventArgs e) => DetailHelpers.FillTipOnOpen(sender);
}
