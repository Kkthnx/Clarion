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
        BuildGroups();
        AppServices.Instance.ExpertModeChangedByUser += OnExpertChanged;
        foreach (var i in _svc.Items) i.CheckedChanged += OnCheckedChanged;
        _svc.PropertyChanged += OnServiceChanged;
        // The service and the rows outlive this page, which is made again on every visit.
        Unloaded += (_, _) =>
        {
            AppServices.Instance.ExpertModeChangedByUser -= OnExpertChanged;
            foreach (var i in _svc.Items) i.CheckedChanged -= OnCheckedChanged;
            _svc.PropertyChanged -= OnServiceChanged;
        };
    }

    private void OnExpertChanged(object? sender, EventArgs e) => DispatcherQueue.TryEnqueue(BuildGroups);
    private void OnCheckedChanged(object? sender, EventArgs e) => DispatcherQueue.TryEnqueue(UpdateSelected);
    private void OnServiceChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => DispatcherQueue.TryEnqueue(Refresh);

    /// <summary>Rows marked for Expert mode only are not listed otherwise.</summary>
    private void BuildGroups()
    {
        var expert = AppServices.Instance.ExpertMode;
        Groups.ItemsSource = _svc.Items.Where(i => expert || !i.Target.ExpertOnly)
            .GroupBy(i => i.Group).Select(g => new CleanupGroup(g.Key, g.ToList())).ToList();
    }

    /// <summary>Shows the files set for the next restart, or after a preview the files that would be. Other programs' entries are only counted.</summary>
    private void ShowQueue()
    {
        if (_svc.LastWasPreview && _svc.LastQueuedFiles.Count > 0)
        {
            QueueTitle.Text = $"{_svc.LastQueuedFiles.Count:N0} files are in use";
            QueueNote.Text = "Nothing was changed. A real clean would set these to be removed when Windows restarts.";
            QueueList.Text = string.Join("\n", _svc.LastQueuedFiles);
            CancelQueueButton.Visibility = Visibility.Collapsed;
            QueueCard.Visibility = Visibility.Visible;
            return;
        }

        QueueView view;
        try { view = _svc.Queue.View(); }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or System.Security.SecurityException)
        {
            QueueCard.Visibility = Visibility.Collapsed;
            return;
        }
        if (view.Waiting.Count == 0) { QueueCard.Visibility = Visibility.Collapsed; return; }

        QueueTitle.Text = $"{view.Waiting.Count:N0} files will be removed when Windows restarts";
        QueueNote.Text = view.OtherOperations > 0
            ? $"Other programs have {view.OtherOperations:N0} more operations waiting for the restart. Clarion never touches those."
            : "Nothing else is waiting for the restart.";
        QueueList.Text = string.Join("\n", view.Waiting.Select(w => w.Path));
        CancelQueueButton.Visibility = Visibility.Visible;
        QueueCard.Visibility = Visibility.Visible;
    }

    private async void OnCancelQueue(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "Cancel these deletes?",
            Content = "The files stay where they are. Windows will not remove them at the next restart. Entries from other programs are not touched.",
            PrimaryButtonText = "Cancel the deletes",
            CloseButtonText = "Keep them queued",
            XamlRoot = XamlRoot,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;
        try
        {
            var removed = _svc.Queue.Cancel();
            ResultBar.Severity = InfoBarSeverity.Success;
            ResultBar.Title = "Cancelled";
            ResultBar.Message = $"{removed:N0} files were taken off the restart list.";
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException or System.Security.SecurityException)
        {
            ResultBar.Severity = InfoBarSeverity.Error;
            ResultBar.Title = "Could not cancel";
            ResultBar.Message = ex.Message;
        }
        ResultBar.IsOpen = true;
        ShowQueue();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Refresh();
        ShowQueue();
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
        // Before the first scan nothing has a size yet, and "0 bytes" would read as if there was nothing to gain.
        var scanned = _svc.Items.Any(i => i.Scan is not null);
        SelectedText.Text = chosen == 0 ? "Nothing selected"
            : scanned ? $"{chosen} selected, {ByteSize.Format(_svc.SelectedBytes)}"
            : $"{chosen} selected, not scanned yet";
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
        var expert = AppServices.Instance.ExpertMode;
        var chosen = _svc.Items.Where(i => i.IsChecked && i.CanSelect && (expert || !i.Target.ExpertOnly)).ToList();
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
        ShowQueue();
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

    private ListView? _selectedList;

    /// <summary>Each group is its own list, so picking a row in one clears the highlight in the others.</summary>
    private void OnRowSelected(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListView list || list.SelectedItem is not CleanupItem item) return;
        if (_selectedList is not null && !ReferenceEquals(_selectedList, list))
        {
            var previous = _selectedList;
            _selectedList = list;
            previous.SelectedItem = null;
        }
        _selectedList = list;
        Pane.Show(item, technical: true);
    }

    private void OnTipOpened(object sender, RoutedEventArgs e) => DetailHelpers.FillTipOnOpen(sender);
}
