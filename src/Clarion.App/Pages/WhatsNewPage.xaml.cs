using Clarion.App.Controls;
using Clarion.App.Services;
using Clarion.Core.Updates;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed record PillVm(string Text, ChipKind Kind);

public sealed partial class WhatsNewPage : Page
{
    private const int History = 5;
    private readonly AppServices _app = AppServices.Instance;
    private int _historyMax = History;

    public WhatsNewPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        // Looking at the page is what "I saw what changed" means.
        _app.AcknowledgeUpdated();
        Build();
    }

    private void Build()
    {
        var current = AppServices.Version;
        var newer = _app.NewerReleases;

        UpdateCard.Visibility = newer.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        NewerSection.Visibility = newer.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        UpToDate.Visibility = newer.Count > 0 ? Visibility.Collapsed : Visibility.Visible;

        if (newer.Count > 0)
        {
            var latest = newer[0];
            UpdateSubtitle.Text = $".Clarion {latest.Version}";
            FromChip.Text = current;
            ToChip.Text = latest.Version;
            ReleaseCount.Text = newer.Count == 1 ? "1 new release" : $"{newer.Count} new releases";
            Pills.ItemsSource = WhatsNew.Totals(newer)
                .Select(t => new PillVm($"{t.Value} {ReleaseViews.LabelFor(t.Key)}", ReleaseViews.ChipFor(t.Key))).ToList();
            NewerTitle.Text = newer.Count == 1 ? "What is new" : "What you missed";
            NewerTimeline.Releases = newer.Select((n, i) => ReleaseViews.From(n, installed: false, expanded: i == 0)).ToList();
            ShowUpdateChoice();
        }
        else
        {
            var last = _app.Settings.LastUpdateCheck;
            StatusText.Text = last is null
                ? ".Clarion has not checked for updates. The button asks GitHub, and only when you press it."
                : $"You have the newest version ({current}). Last checked {last.Value.LocalDateTime:MMM d, h:mm tt}.";
        }

        var history = WhatsNew.InstalledAndOlder(current, ChangelogParser.LoadEmbedded(), _historyMax, out var leftOut);
        HistoryTitle.Text = newer.Count > 0 ? "Your version and earlier" : "Your version and earlier releases";
        HistoryTimeline.Releases = history
            .Select((n, i) => ReleaseViews.From(n, installed: i == 0 && n.Version == current, expanded: i == 0 && newer.Count == 0))
            .ToList();
        ShowOlder.Visibility = leftOut > 0 ? Visibility.Visible : Visibility.Collapsed;
        ShowOlder.Content = leftOut == 1 ? "Show 1 older release" : $"Show {leftOut} older releases";
    }

    private async void OnCheck(object sender, RoutedEventArgs e)
    {
        CheckButton.IsEnabled = false;
        Busy.IsActive = true;
        StatusText.Text = "Asking GitHub";
        string message;
        try
        {
            message = await _app.CheckForUpdateAsync(manual: true);
        }
        finally
        {
            Busy.IsActive = false;
            CheckButton.IsEnabled = true;
        }
        Build();
        if (_app.NewerReleases.Count == 0) StatusText.Text = message;
    }

    // Stops the download that is running, or null. The token source itself lives only as long as the download.
    private Action? _stopDownload;

    /// <summary>Update now when this copy can update itself. Otherwise the release page is the way, and the note says why.</summary>
    private void ShowUpdateChoice()
    {
        var inApp = _app.CanUpdateInApp;
        UpdateNowButton.Visibility = inApp ? Visibility.Visible : Visibility.Collapsed;
        OpenPageButton.Style = inApp ? null : (Style)Application.Current.Resources["AccentButtonStyle"];
        UpdateNote.Text = inApp
            ? "Update now downloads the setup program from this project's GitHub release, checks it against the SHA-256 GitHub lists for it, then closes .Clarion, installs the update and opens it again. Your settings and history are kept."
            : _app.UpdateAvailable?.Release is { } r && Clarion.Core.Updates.UpdateInstall.SetupOf(r) is null
                ? "This release has no setup program .Clarion can install by itself. The button opens the page in your browser."
                : "This copy is not the one the setup program installed, such as the portable zip, so it is updated from the release page. The button opens it in your browser.";
    }

    private async void OnUpdateNow(object sender, RoutedEventArgs e)
    {
        if (_app.IsChangingThisPc)
        {
            ShowUpdateStatus(".Clarion is changing settings right now. Wait for it to finish, then update.");
            return;
        }
        if (_app.PendingCount > 0)
        {
            var ask = new ContentDialog
            {
                Title = "Changes are still queued",
                Content = $"{(_app.PendingCount == 1 ? "1 queued change is" : $"{_app.PendingCount} queued changes are")} not applied yet and will be cleared when .Clarion closes to update. Nothing on this PC changes.",
                PrimaryButtonText = "Update anyway",
                CloseButtonText = "Not now",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot,
            };
            if (await Dialogs.ShowWhenFree(ask) != ContentDialogResult.Primary) return;
        }

        using var download = new CancellationTokenSource();
        _stopDownload = download.Cancel;
        UpdateButtons.Visibility = Visibility.Collapsed;
        DownloadPanel.Visibility = Visibility.Visible;
        DownloadBar.Value = 0;
        ShowUpdateStatus("Downloading the update.");
        var progress = new Progress<double>(p => DownloadBar.Value = p);
        try
        {
            var setup = await _app.DownloadUpdateAsync(progress, download.Token);
            ShowUpdateStatus("The download matches its checksum. .Clarion closes now, installs the update and opens again.");
            CancelDownloadButton.IsEnabled = false;
            // A moment for the sentence to be read and announced before the window goes.
            await Task.Delay(1500);
            if (_app.IsChangingThisPc || !AppServices.StartUpdate(setup))
            {
                ShowUpdateStatus(_app.IsChangingThisPc
                    ? ".Clarion started changing settings, so the update waits. Press Update now again when it is done."
                    : "The setup program could not be started. The release page has it, and the log has the reason.");
                ResetUpdateButtons();
                return;
            }
            Application.Current.Exit();
        }
        catch (OperationCanceledException) when (download.IsCancellationRequested)
        {
            ShowUpdateStatus("The download was stopped. Nothing was installed.");
            ResetUpdateButtons();
        }
        catch (UpdateCheckException ex)
        {
            Log.Write($"Update: {ex.Message}");
            ShowUpdateStatus(ex.Message);
            ResetUpdateButtons();
        }
        finally
        {
            _stopDownload = null;
        }
    }

    private void OnCancelDownload(object sender, RoutedEventArgs e) => _stopDownload?.Invoke();

    private void ShowUpdateStatus(string text)
    {
        UpdateStatus.Text = text;
        UpdateStatus.Visibility = Visibility.Visible;
    }

    private void ResetUpdateButtons()
    {
        DownloadPanel.Visibility = Visibility.Collapsed;
        CancelDownloadButton.IsEnabled = true;
        UpdateButtons.Visibility = Visibility.Visible;
    }

    // A download still running when the page is left is stopped, so it does not finish and close the program behind the person's back.
    protected override void OnNavigatedFrom(NavigationEventArgs e) => _stopDownload?.Invoke();

    private void OnOpenLatest(object sender, RoutedEventArgs e)
    {
        if (_app.UpdateAvailable is { } offer && UpdateCheck.IsTrustedReleaseUrl(offer.Url)) Shell.Open(offer.Url);
    }

    private void OnHide(object sender, RoutedEventArgs e)
    {
        var hidden = _app.UpdateAvailable?.Tag;
        _app.HideUpdate();
        Build();
        if (hidden is not null) StatusText.Text = $"{hidden} is hidden. Press the button to look again and show it.";
    }

    private void OnShowOlder(object sender, RoutedEventArgs e)
    {
        _historyMax = int.MaxValue;
        Build();
    }
}
