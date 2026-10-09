using System.Diagnostics;
using Clarion.App.Pages;
using Clarion.App.Services;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;

namespace Clarion.App;

public sealed partial class MainWindow : Window
{
    private readonly AppServices _app = AppServices.Instance;

    public ActivityLog Activity => _app.Activity;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Clarion";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        ResizeToFit();
        AppWindow.Closing += OnClosing;
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "Clarion.ico"));
        Root.ActualThemeChanged += (_, _) => UpdateCaptionButtons();
        UpdateCaptionButtons();

        Activity.PropertyChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateActivity);
        Activity.Lines.CollectionChanged += (_, _) => DispatcherQueue.TryEnqueue(ScrollActivity);

        _app.PropertyChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateBar);
        _app.Pending.CollectionChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateBar);
        _app.PropertyChanged += (_, a) => { if (a.PropertyName == nameof(AppServices.DriftCount)) DispatcherQueue.TryEnqueue(UpdateVerifyBadge); };
        _app.NavigateRequested += (_, tag) => DispatcherQueue.TryEnqueue(() => GoTo(tag));
        _app.ReviewRequested += (_, _) => DispatcherQueue.TryEnqueue(() => OnReview(this, new RoutedEventArgs()));

        var refresh = new Microsoft.UI.Xaml.Input.KeyboardAccelerator { Key = Windows.System.VirtualKey.F5 };
        refresh.Invoked += (_, a) => { OnRefresh(this, new RoutedEventArgs()); a.Handled = true; };
        Root.KeyboardAccelerators.Add(refresh);
        Nav.SelectedItem = Nav.MenuItems[0];
        UpdateBar();
        _ = _app.RefreshStatesAsync();
    }

    private async void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (!_app.IsBusy) return;
        args.Cancel = true;
        await new ContentDialog
        {
            Title = "Clarion is still working",
            Content = "Closing now could leave a change half done. Wait for it to finish, then close the window.",
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot,
        }.ShowAsync();
    }

    private void OnGlobalSearch(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var q = args.QueryText.Trim();
        if (q.Length == 0) return;
        Nav.SelectedItem = null;
        ContentFrame.Navigate(typeof(TweakListPage), new PageArgs("Search results", $"Settings matching \"{q}\"", [], q));
    }

    public void ApplyTheme(ElementTheme theme)
    {
        Root.RequestedTheme = theme;
        UpdateCaptionButtons();
    }

    /// <summary>The minimize, maximize and close buttons draw on top of our own title bar, so they need to follow the theme.</summary>
    private void UpdateCaptionButtons()
    {
        var bar = AppWindow.TitleBar;
        var dark = Root.ActualTheme != ElementTheme.Light;
        var fg = dark ? Windows.UI.Color.FromArgb(255, 230, 235, 242) : Windows.UI.Color.FromArgb(255, 19, 26, 36);
        var hover = dark ? Windows.UI.Color.FromArgb(255, 36, 48, 64) : Windows.UI.Color.FromArgb(255, 214, 222, 232);
        bar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        bar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
        bar.ButtonForegroundColor = fg;
        bar.ButtonHoverForegroundColor = fg;
        bar.ButtonHoverBackgroundColor = hover;
        bar.ButtonPressedForegroundColor = fg;
        bar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(255, 122, 135, 153);
    }

    private void ResizeToFit()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var appWindow = AppWindow.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd));
        var area = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        var width = Math.Min(1360, area.Width - 80);
        var height = Math.Min(900, area.Height - 80);
        appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
    }

    private static readonly Dictionary<string, PageArgs> Sections = new()
    {
        ["privacy"] = new("Privacy", "Stop Windows from collecting and sharing more than it needs to.", ["Privacy", "Security"]),
        ["debloat"] = new("Debloat", "Remove apps you do not use. Every removal can be reverted.", ["Debloat"]),
        ["tweaks"] = new("Tweaks", "Look, feel and everyday behavior of Windows.", ["Appearance", "Explorer", "Input", "System"]),
        ["power"] = new("Power", "Pick how the PC trades speed for energy. Only one power plan is active at a time.", ["Power"]),
        ["network"] = new("Network", "Update sharing, public DNS providers and encrypted DNS.", ["Network"]),
        ["features"] = new("Windows features", "Optional parts of Windows you can turn on or off.", ["Features"]),
        ["updates"] = new("Updates", "Keep updates predictable without turning off protection.", ["Updates"]),
    };

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
            return;
        }

        var tag = (args.SelectedItem as NavigationViewItem)?.Tag as string;
        switch (tag)
        {
            case "home": ContentFrame.Navigate(typeof(HomePage)); break;
            case "feedback": ContentFrame.Navigate(typeof(FeedbackPage)); break;
            case "system": ContentFrame.Navigate(typeof(SystemPage)); break;
            case "troubleshoot": ContentFrame.Navigate(typeof(TroubleshootPage)); break;
            case "verify": ContentFrame.Navigate(typeof(VerifyPage)); break;
            case "safety": ContentFrame.Navigate(typeof(SafetyPage)); break;
            case "cleanup": ContentFrame.Navigate(typeof(CleanupPage)); break;
            case "repair": ContentFrame.Navigate(typeof(RepairPage)); break;
            case not null when Sections.TryGetValue(tag, out var args2): ContentFrame.Navigate(typeof(TweakListPage), args2); break;
        }
    }

    private void UpdateActivity()
    {
        ActivityPanel.Visibility = Activity.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        ActivityBar.IsIndeterminate = Activity.Indeterminate;
        ActivityBar.Maximum = Math.Max(1, Activity.Total);
        ActivityBar.Value = Activity.Value;
        ActivityHide.IsEnabled = !Activity.IsRunning;
    }

    private void ScrollActivity()
    {
        ActivityScroll.UpdateLayout();
        ActivityScroll.ChangeView(null, ActivityScroll.ScrollableHeight, null, disableAnimation: true);
    }

    private void OnHideActivity(object sender, RoutedEventArgs e) => Activity.Close();

    private void OnOpenLog(object sender, RoutedEventArgs e) =>
        Process.Start(new ProcessStartInfo("notepad.exe", $"\"{Log.Path}\"") { UseShellExecute = true });

    private void UpdateBar()
    {
        var count = _app.PendingCount;
        BusyRing.IsActive = _app.IsBusy;
        StatusText.Text = _app.IsBusy ? _app.Status
            : count == 0 ? _app.Status
            : count == 1 ? "1 change queued" : $"{count} changes queued";
        ReviewButton.IsEnabled = count > 0 && !_app.IsBusy;
        ClearButton.IsEnabled = count > 0 && !_app.IsBusy;
        RefreshButton.IsEnabled = !_app.IsBusy;
    }

    private void UpdateVerifyBadge()
    {
        VerifyBadge.Value = _app.DriftCount;
        VerifyBadge.Visibility = _app.DriftCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void GoTo(string tag)
    {
        var target = Nav.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(i => i.Tag as string == tag);
        if (target is not null) Nav.SelectedItem = target;
    }

    private void OnClear(object sender, RoutedEventArgs e) => _app.ClearPending();

    private async void OnRefresh(object sender, RoutedEventArgs e)
    {
        if (!_app.IsBusy) await _app.RefreshStatesAsync(fresh: true);
    }

    private async void OnReview(object sender, RoutedEventArgs e)
    {
        if (_app.PendingCount == 0) return;

        var list = new StackPanel { Spacing = 12 };
        foreach (var item in _app.Pending)
        {
            var block = new StackPanel { Spacing = 2 };
            block.Children.Add(new TextBlock
            {
                Text = $"{item.Name}  ({(item.IsOn ? "turn on" : "revert")})",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
            });
            if (item.IsOn)
            {
                foreach (var step in item.ExactChanges)
                    block.Children.Add(new TextBlock { Text = step, FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Cascadia Mono"), FontSize = 12, TextWrapping = TextWrapping.Wrap, Opacity = 0.8 });
            }
            else
            {
                block.Children.Add(new TextBlock { Text = "Puts back the value that was recorded before the change.", FontSize = 12, Opacity = 0.8 });
            }
            list.Children.Add(block);
        }

        var restore = new CheckBox { Content = "Create a restore point first (recommended)", IsChecked = true };
        var body = new StackPanel { Spacing = 14 };
        body.Children.Add(new ScrollViewer { MaxHeight = 380, Content = list });
        body.Children.Add(restore);

        var dialog = new ContentDialog
        {
            Title = $"Apply {_app.PendingCount} change{(_app.PendingCount == 1 ? "" : "s")}?",
            Content = body,
            PrimaryButtonText = "Apply",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = Content.XamlRoot,
        };
        if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

        await RunAsync(restore.IsChecked == true);
    }

    private async Task RunAsync(bool restorePoint, bool turnOnProtection = false)
    {
        var restartsExplorer = _app.Pending.Any(p => p.Tweak.RestartsExplorer);
        var needsReboot = _app.Pending.Any(p => p.Tweak.NeedsReboot);
        var needsSignOut = _app.Pending.Any(p => p.Tweak.NeedsSignOut);

        Activity.Start(restorePoint ? "Applying changes, making a restore point first" : "Applying changes");
        var result = await _app.ApplyPendingAsync(restorePoint,
            msg => DispatcherQueue.TryEnqueue(() => { _app.Status = msg; Activity.Info(msg); }),
            step => DispatcherQueue.TryEnqueue(() => Activity.Step(step)),
            turnOnProtection);
        if (result.Blocked is not null) Activity.Close();
        else Activity.Finish(result.Items.Count(i => i.Result.Success && !i.Skipped), result.Items.Count(i => !i.Result.Success), result.Items.Count(i => i.Skipped && i.Result.Success));

        if (result.Blocked is not null)
        {
            var retry = new ContentDialog
            {
                Title = "No restore point, nothing was changed",
                Content = result.Blocked + (turnOnProtection
                    ? "\n\nSystem Protection could not be turned on, or the restore point still failed. A policy on this PC can block it."
                    : "\n\nSystem Protection is often off on a new Windows install. Clarion can turn it on for your system drive and try again. Restore points keep some disk space."),
                PrimaryButtonText = turnOnProtection ? "" : "Turn on System Protection and retry",
                SecondaryButtonText = "Continue without one",
                CloseButtonText = "Cancel",
                DefaultButton = turnOnProtection ? ContentDialogButton.Close : ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot,
            };
            var choice = await retry.ShowAsync();
            if (choice == ContentDialogResult.Primary) await RunAsync(restorePoint: true, turnOnProtection: true);
            else if (choice == ContentDialogResult.Secondary) await RunAsync(restorePoint: false);
            return;
        }

        var anyFailed = result.Items.Any(i => !i.Result.Success);
        if (anyFailed || restartsExplorer || needsReboot || needsSignOut)
            await ShowSummaryAsync(result, restartsExplorer, needsReboot, needsSignOut);
    }

    private async Task ShowSummaryAsync(Clarion.Core.Engine.BatchResult result, bool restartsExplorer, bool needsReboot, bool needsSignOut)
    {
        var done = result.Items.Count(i => i.Result.Success && !i.Skipped);
        var failed = result.Items.Where(i => !i.Result.Success).ToList();

        var body = new StackPanel { Spacing = 10 };
        var untouched = result.Items.Count(i => i.Skipped && i.Result.Success);
        var rest = untouched > 0 ? $" {untouched} {(untouched == 1 ? "was" : "were")} left as {(untouched == 1 ? "it was" : "they were")} (already on)." : "";
        body.Children.Add(new TextBlock { Text = (failed.Count == 0 ? $"{done} change{(done == 1 ? "" : "s")} done." : $"{done} done, {failed.Count} did not finish. The others ran as normal.") + rest, TextWrapping = TextWrapping.Wrap });
        foreach (var f in failed)
        {
            var name = _app.Items.FirstOrDefault(i => i.Id == f.TweakId)?.Name ?? f.TweakId;
            body.Children.Add(new TextBlock { Text = $"{name}: {f.Result.Error}", TextWrapping = TextWrapping.Wrap, Opacity = 0.85, FontSize = 12 });
        }
        if (needsReboot) body.Children.Add(new TextBlock { Text = "Restart Windows to finish some of these.", TextWrapping = TextWrapping.Wrap });
        if (needsSignOut) body.Children.Add(new TextBlock { Text = "Sign out and back in to finish some of these.", TextWrapping = TextWrapping.Wrap });

        var dialog = new ContentDialog
        {
            Title = failed.Count == 0 ? "All done" : "Finished with problems",
            Content = body,
            CloseButtonText = "Close",
            XamlRoot = Content.XamlRoot,
        };
        if (restartsExplorer)
        {
            body.Children.Add(new TextBlock { Text = "Explorer needs a restart to show some changes.", TextWrapping = TextWrapping.Wrap });
            dialog.PrimaryButtonText = "Restart Explorer now";
        }
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) RestartExplorer();
    }

    private static void RestartExplorer()
    {
        foreach (var p in Process.GetProcessesByName("explorer"))
        {
            try { p.Kill(); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { }
        }
    }
}
