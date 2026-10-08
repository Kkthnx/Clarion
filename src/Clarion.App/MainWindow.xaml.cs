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

    public MainWindow()
    {
        InitializeComponent();
        Title = "Clarion";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        ResizeToFit();

        _app.PropertyChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateBar);
        _app.Pending.CollectionChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateBar);

        Nav.SelectedItem = Nav.MenuItems[0];
        UpdateBar();
        _ = _app.RefreshStatesAsync();
    }

    public void ApplyTheme(ElementTheme theme) => Root.RequestedTheme = theme;

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
        ["network"] = new("Network", "Update sharing and connection behavior.", ["Network"]),
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
            case "safety": ContentFrame.Navigate(typeof(SafetyPage)); break;
            case not null when Sections.TryGetValue(tag, out var args2): ContentFrame.Navigate(typeof(TweakListPage), args2); break;
        }
    }

    private void UpdateBar()
    {
        var count = _app.PendingCount;
        BusyRing.IsActive = _app.IsBusy;
        StatusText.Text = _app.IsBusy ? _app.Status
            : count == 0 ? _app.Status
            : count == 1 ? "1 change queued" : $"{count} changes queued";
        ReviewButton.IsEnabled = count > 0 && !_app.IsBusy;
        ClearButton.IsEnabled = count > 0 && !_app.IsBusy;
    }

    private void OnClear(object sender, RoutedEventArgs e) => _app.ClearPending();

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

    private async Task RunAsync(bool restorePoint)
    {
        var restartsExplorer = _app.Pending.Any(p => p.Tweak.RestartsExplorer);
        var needsReboot = _app.Pending.Any(p => p.Tweak.NeedsReboot);
        var needsSignOut = _app.Pending.Any(p => p.Tweak.NeedsSignOut);

        var result = await _app.ApplyPendingAsync(restorePoint, msg => DispatcherQueue.TryEnqueue(() => _app.Status = msg));

        if (result.Blocked is not null)
        {
            var retry = new ContentDialog
            {
                Title = "No restore point, nothing was changed",
                Content = result.Blocked + "\n\nYou can turn on System Protection for your system drive and try again, or continue without a restore point.",
                PrimaryButtonText = "Continue without one",
                CloseButtonText = "Cancel",
                XamlRoot = Content.XamlRoot,
            };
            if (await retry.ShowAsync() == ContentDialogResult.Primary) await RunAsync(restorePoint: false);
            return;
        }

        await ShowSummaryAsync(result, restartsExplorer, needsReboot, needsSignOut);
    }

    private async Task ShowSummaryAsync(Clarion.Core.Engine.BatchResult result, bool restartsExplorer, bool needsReboot, bool needsSignOut)
    {
        var done = result.Items.Count(i => i.Result.Success && !i.Skipped);
        var failed = result.Items.Where(i => !i.Result.Success).ToList();

        var body = new StackPanel { Spacing = 10 };
        body.Children.Add(new TextBlock { Text = failed.Count == 0 ? $"{done} change{(done == 1 ? "" : "s")} done." : $"{done} done, {failed.Count} did not finish.", TextWrapping = TextWrapping.Wrap });
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
