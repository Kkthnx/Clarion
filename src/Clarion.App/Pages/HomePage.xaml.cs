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

    private static readonly (string Key, string Title, string Text)[] PresetInfo =
    [
        ("minimal", "Minimal", "Stop promotions and drop diagnostic data to the lowest level your edition allows. The gentlest choice."),
        ("standard", "Standard", "Minimal plus private search, no activity history, no update sharing and a few Explorer basics. Best for most people."),
        ("advanced", "Advanced", "Standard plus telemetry services and tasks, location off, classic right-click menu and tidier taskbar."),
        ("privacy", "Privacy max", "Every privacy and promotion setting we suggest for everyone. Only low risk items, no app permission lockdowns."),
        ("gaming", "Gaming", "Pointer, key and menu tweaks, fewer background jobs and no update sharing."),
    ];

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
            }
        };
        _app.PropertyChanged += (_, _) => DispatcherQueue.TryEnqueue(Refresh);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) => Refresh();

    private void Refresh()
    {
        BuildText.Text = _app.Profile.Build.ToString();
        EditionText.Text = _app.Profile.Edition;
        AdminText.Text = _app.IsElevated ? "Yes" : "No";
        AppliedText.Text = $"{_app.AppliedCount} of {_app.Items.Count(i => i.IsSupported)}";
        Busy.IsActive = _app.IsBusy;
        ShowDrift();
    }

    private void ShowDrift()
    {
        var report = _app.Drift;
        DriftBar.IsOpen = report is { HasDrift: true };
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

    private void OnReviewDrift(object sender, RoutedEventArgs e) => _app.RequestNavigate("verify");

    private void BuildPresets()
    {
        foreach (var (key, title, text) in PresetInfo)
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
