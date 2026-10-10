using Clarion.App.Controls;
using Clarion.App.Services;
using Clarion.Engine;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed partial class HomePage : Page
{
    private readonly AppServices _app = AppServices.Instance;

    public HomePage()
    {
        InitializeComponent();
        Loaded += async (_, _) =>
        {
            if (PresetList.Items.Count == 0) BuildPresets();
            Refresh();
            var account = await Task.Run(AccountCheck.Detect);
            if (account.Differs)
            {
                AccountBar.Message = $"You are signed in as {account.SignedInAs}, but Clarion was started with {account.ElevatedAs}. " +
                    "Settings that apply to one account will change that other account. Close Clarion and start it again from your own account.";
                AccountBar.IsOpen = true;
                AccountBar.Visibility = AccountBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
            }
        };
        _app.PropertyChanged += OnAppChanged;
        // A new page is made on every visit, so the handler on the app-wide object must go when the page does.
        Unloaded += (_, _) => _app.PropertyChanged -= OnAppChanged;
    }

    private void OnAppChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => DispatcherQueue.TryEnqueue(Refresh);

    private async void OnFixMonthly(object sender, RoutedEventArgs e)
    {
        var problem = await _app.RepairMonthlyCheckAsync();
        if (problem is not null)
        {
            MovedBar.Severity = InfoBarSeverity.Error;
            MovedBar.Message = $"Could not set it up again. {problem}";
        }
    }

    private void OnRestartAsAdmin(object sender, RoutedEventArgs e)
    {
        // A run in progress must finish first. A queue that has not been applied is not carried over to the new window.
        if (_app.IsBusy) return;
        ((App)Application.Current).RestartAsAdministrator();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) => Refresh();

    private void Refresh()
    {
        BuildText.Text = _app.Profile.Build.ToString();
        EditionText.Text = _app.Profile.Edition;
        AdminText.Text = _app.IsElevated ? "Yes" : "No";
        AppliedText.Text = $"{_app.AppliedCount} of {_app.Items.Count(i => i.IsSupported)}";
        Busy.IsActive = _app.IsBusy;
        MovedBar.IsOpen = _app.MonthlyCheckMoved;
        MovedBar.Visibility = MovedBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        LimitedBar.IsOpen = !_app.IsElevated;
        LimitedBar.Visibility = LimitedBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        ShowUpdated();
        ShowUpdate();
        ShowScheduled();
        ShowDrift();
        ShowRestart();
        ShowImage();
    }

    private void ShowDrift()
    {
        var report = _app.Drift;
        DriftBar.IsOpen = report is { HasDrift: true };
        DriftBar.Visibility = DriftBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (report is not { HasDrift: true }) return;
        var apps = report.ReturnedApps.Count;
        var rest = report.ChangedBack.Count;
        DriftBar.Title = apps > 0 && rest > 0 ? "Apps came back and settings were changed back"
            : apps > 0 ? (apps == 1 ? "A removed app came back" : "Removed apps came back")
            : (rest == 1 ? "A setting was changed back" : "Settings were changed back");
        DriftBar.Message = report.FeatureUpdateSinceLastScan
            ? "This is common after a Windows feature update. You can put them back in one step."
            : "You can put them back in one step, or leave them as they are.";
    }

    private void ShowScheduled()
    {
        var result = _app.UnseenScheduledResult();
        ScheduledBar.IsOpen = result is not null;
        ScheduledBar.Visibility = ScheduledBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (result is null) return;
        ScheduledBar.Message = $"On {result.At.LocalDateTime:MMM d}, {result.Attention} of the {result.Checked} settings Clarion applied " +
            $"had been changed back. Review shows which and lets you put them back.";
    }

    private void OnReviewScheduled(object sender, RoutedEventArgs e)
    {
        _app.MarkScheduledSeen();
        _app.RequestNavigate("verify");
        ShowScheduled();
    }

    private void ShowUpdated()
    {
        var from = _app.UpdatedFromVersion;
        UpdatedBar.IsOpen = from is not null;
        UpdatedBar.Visibility = UpdatedBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (from is null) return;
        UpdatedBar.Title = $"Clarion was updated to {AppServices.Version}";
        UpdatedBar.Message = $"You were on {from}.";
    }

    private void OnUpdatedClosed(InfoBar sender, InfoBarClosedEventArgs args) => _app.AcknowledgeUpdated();

    private void OnSeeWhatsNew(object sender, RoutedEventArgs e) => _app.RequestNavigate("whatsnew");

    private void ShowUpdate()
    {
        var offer = _app.UpdateAvailable;
        UpdateBar.IsOpen = offer is not null;
        UpdateBar.Visibility = UpdateBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (offer is null) return;
        UpdateBar.Title = $"{offer.Name} is available";
        var count = _app.NewerReleases.Count;
        UpdateBar.Message = count > 1
            ? $"You have {AppServices.Version}, which is {count} releases behind. Clarion does not download or install anything itself."
            : $"You have {AppServices.Version}. Clarion does not download or install anything itself.";
    }

    private void OnOpenUpdate(object sender, RoutedEventArgs e)
    {
        // The address came from the network, so it is only opened when it points at this project's releases.
        if (_app.UpdateAvailable is { } offer && Clarion.Core.Updates.UpdateCheck.IsTrustedReleaseUrl(offer.Url)) Shell.Open(offer.Url);
    }

    private void OnHideUpdate(object sender, RoutedEventArgs e) => _app.HideUpdate();

    private void ShowRestart()
    {
        var pending = _app.PendingRestartNow;
        RestartBar.IsOpen = pending is { IsPending: true };
        RestartBar.Visibility = RestartBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (pending is not { IsPending: true }) return;

        var checkedCount = _app.Drift?.Checked ?? 0;
        RestartBar.Message = $"{string.Join(". ", pending.Reasons)}. " +
            (checkedCount > 0
                ? $"After it restarts, Clarion will check the {checkedCount} setting{(checkedCount == 1 ? "" : "s")} it applied again, because updates are when changes are most likely to be undone."
                : "Once you have applied settings, Clarion checks them again after a restart like this.");
    }

    private void ShowImage()
    {
        var verdict = _app.ImageVerdict;
        ImageBar.IsOpen = verdict is { Level: not Clarion.Core.SystemInfo.InstallLevel.Standard };
        ImageBar.Visibility = ImageBar.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        if (verdict is not { Level: not Clarion.Core.SystemInfo.InstallLevel.Standard }) return;

        var named = verdict.ImageName is { } name ? $" ({name})" : "";
        ImageBar.Title = verdict.Headline + named;
        ImageBar.Message = _app.ImageNoteCount > 0
            ? $"{_app.ImageNoteCount} settings have a note about this. Settings whose target is already gone show as having nothing to change."
            : "Settings whose target is already gone show as having nothing to change. Clarion only changes what you choose.";
    }

    private void OnSeeImage(object sender, RoutedEventArgs e) => _app.RequestNavigate("system");

    private void OnReviewDrift(object sender, RoutedEventArgs e) => _app.RequestNavigate("verify");

    private void BuildPresets()
    {
        foreach (var (key, title, text) in Services.PresetInfo.All)
        {
            var queue = new Button { Content = "Queue this preset", Style = (Style)Application.Current.Resources["AccentButtonStyle"] };
            queue.Click += (_, _) =>
            {
                var added = _app.QueuePreset(key);
                ToolTipService.SetToolTip(queue, added == 0 ? "Everything in this preset is already on" : $"{added} settings queued");
            };

            var count = _app.Presets.TryGetValue(key, out var ids) ? ids.Count : 0;
            var card = new Border
            {
                Background = ThemeBrushes.Get("ClSurface1", ActualTheme),
                BorderBrush = ThemeBrushes.Get("ClBorder", ActualTheme),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(20),
                Child = BuildCardContent(title, text, count, queue),
            };
            PresetList.Items.Add(card);
        }
    }

    private Grid BuildCardContent(string title, string text, int count, Button action)
    {
        var grid = new Grid { ColumnSpacing = 20 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var left = new StackPanel { Spacing = 4 };
        left.Children.Add(new TextBlock { Text = title, FontSize = 18, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, Foreground = ThemeBrushes.Get("ClTextPrimary", ActualTheme) });
        left.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Foreground = ThemeBrushes.Get("ClTextSecondary", ActualTheme) });
        left.Children.Add(new TextBlock { Text = $"{count} settings", FontSize = 12, Foreground = ThemeBrushes.Get("ClTextMuted", ActualTheme) });
        grid.Children.Add(left);

        Grid.SetColumn(action, 1);
        action.VerticalAlignment = VerticalAlignment.Center;
        grid.Children.Add(action);
        return grid;
    }
}
