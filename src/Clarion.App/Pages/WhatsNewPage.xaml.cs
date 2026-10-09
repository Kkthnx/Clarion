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
            UpdateSubtitle.Text = $"Clarion {latest.Version}";
            FromChip.Text = current;
            ToChip.Text = latest.Version;
            ReleaseCount.Text = newer.Count == 1 ? "1 new release" : $"{newer.Count} new releases";
            Pills.ItemsSource = WhatsNew.Totals(newer)
                .Select(t => new PillVm($"{t.Value} {ReleaseViews.LabelFor(t.Key)}", ReleaseViews.ChipFor(t.Key))).ToList();
            NewerTitle.Text = newer.Count == 1 ? "What is new" : "What you missed";
            NewerTimeline.Releases = newer.Select((n, i) => ReleaseViews.From(n, installed: false, expanded: i == 0)).ToList();
        }
        else
        {
            var last = _app.Settings.LastUpdateCheck;
            StatusText.Text = last is null
                ? "Clarion has not checked for updates. The button asks GitHub, and only when you press it."
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
        var message = await _app.CheckForUpdateAsync(manual: true);
        Busy.IsActive = false;
        CheckButton.IsEnabled = true;
        Build();
        if (_app.NewerReleases.Count == 0) StatusText.Text = message;
    }

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
