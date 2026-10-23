using System.Diagnostics;
using Clarion.App.Controls;
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
        Root.RequestedTheme = ((App)Application.Current).Theme;
        Title = ".Clarion";
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        ResizeToFit();
        AppWindow.Closing += OnClosing;
        Shell.LinkNotOpened += address => DispatcherQueue.TryEnqueue(() => ShowLinkNotOpened(address));
        _app.SafeStateEntered += (_, _) => DispatcherQueue.TryEnqueue(ShowSafeState);
        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "Clarion.ico"));
        Root.ActualThemeChanged += (_, _) => { ThemeBrushes.Current = Root.ActualTheme; UpdateCaptionButtons(); };
        // Before the window is shown, the theme may still read as the system default, so it is read again once it is loaded.
        Root.Loaded += (_, _) => { ThemeBrushes.Current = Root.ActualTheme; UpdateCaptionButtons(); };
        ThemeBrushes.Current = Root.ActualTheme;
        UpdateCaptionButtons();

        Activity.PropertyChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateActivity);
        Activity.Lines.CollectionChanged += (_, _) => DispatcherQueue.TryEnqueue(ScrollActivity);

        _app.PropertyChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateBar);
        _app.Pending.CollectionChanged += (_, _) => DispatcherQueue.TryEnqueue(UpdateBar);
        _app.PropertyChanged += (_, a) => { if (a.PropertyName == nameof(AppServices.DriftCount)) DispatcherQueue.TryEnqueue(UpdateVerifyBadge); };
        _app.PropertyChanged += (_, a) =>
        {
            if (a.PropertyName is nameof(AppServices.NewerReleases) or nameof(AppServices.UpdatedFromVersion)) DispatcherQueue.TryEnqueue(UpdateWhatsNewBadge);
        };
        UpdateWhatsNewBadge();
        _app.NavigateRequested += (_, tag) => DispatcherQueue.TryEnqueue(() => GoTo(tag));
        _app.SearchRequested += (_, q) => DispatcherQueue.TryEnqueue(() => OpenSearch(q));
        _app.ReviewRequested += (_, _) => DispatcherQueue.TryEnqueue(() => OnReview(this, new RoutedEventArgs()));

        var refresh = new Microsoft.UI.Xaml.Input.KeyboardAccelerator { Key = Windows.System.VirtualKey.F5 };
        refresh.Invoked += (_, a) => { OnRefresh(this, new RoutedEventArgs()); a.Handled = true; };
        Root.KeyboardAccelerators.Add(refresh);
        // Ctrl+F goes to the search box from anywhere, as in most programs.
        var find = new Microsoft.UI.Xaml.Input.KeyboardAccelerator { Key = Windows.System.VirtualKey.F, Modifiers = Windows.System.VirtualKeyModifiers.Control };
        find.Invoked += (_, a) => { GlobalSearch.Focus(FocusState.Keyboard); a.Handled = true; };
        Root.KeyboardAccelerators.Add(find);
        // F5 belongs to the whole window. Left alone, Windows pops up a small "F5" tip wherever focus or the pointer lands, which looks
        // like a stray button. The Re-check button's own tooltip already says F5.
        Root.KeyboardAcceleratorPlacementMode = Microsoft.UI.Xaml.Input.KeyboardAcceleratorPlacementMode.Hidden;
        // There is no back button, so keep the frame from collecting a history entry for every page opened.
        ContentFrame.Navigated += (_, _) => ContentFrame.BackStack.Clear();
        Nav.SelectedItem = Nav.MenuItems[0];
        UpdateBar();
        // Left alone, focus lands on the first control that can take it, which is the search box in the title bar, so a caret sits in a
        // field nobody asked for. It starts on the page list instead. Pointer focus draws no focus rectangle, and the first key press does.
        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
            (Nav.ContainerFromMenuItem(Nav.SelectedItem) as Control)?.Focus(FocusState.Pointer));
        _ = StartupAsync();
        _ = _app.LoadImageNotesAsync();
        _ = _app.StartupUpdateCheckAsync();
        _ = _app.CheckMonthlyTaskAsync();
    }

    private bool _safeStateDialogOpen;

    private async void ShowSafeState()
    {
        if (_safeStateDialogOpen || Content?.XamlRoot is null) return;
        _safeStateDialogOpen = true;
        try
        {
            await Dialogs.Say(Content.XamlRoot, "Something unexpected happened",
                ".Clarion hit an error it did not expect. To be safe it emptied the queue and read every setting again from Windows. The error itself changed nothing. " +
                "The details are in crash.log in the .Clarion data folder. If this keeps happening, close .Clarion and open it again.");
        }
        finally { _safeStateDialogOpen = false; }
    }

    private bool _linkDialogOpen;

    /// <summary>Shows a web address that could not be opened safely, ready to copy, instead of opening it with administrator rights.</summary>
    private async void ShowLinkNotOpened(string address)
    {
        if (_linkDialogOpen || Content?.XamlRoot is null) return;
        _linkDialogOpen = true;
        try
        {
            var box = new TextBox { Text = address, IsReadOnly = true, TextWrapping = TextWrapping.Wrap };
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(box, "Web address");
            var body = new StackPanel { Spacing = 10, MaxWidth = 520 };
            body.Children.Add(new TextBlock
            {
                Text = "This link was not opened, because .Clarion has administrator rights and a browser started from it would have them too. Copy the address and open it in your browser.",
                TextWrapping = TextWrapping.Wrap,
            });
            body.Children.Add(box);
            var dialog = new ContentDialog { Title = "Open this link yourself", Content = body, PrimaryButtonText = "Copy address", CloseButtonText = "Close", XamlRoot = Content.XamlRoot };
            if (await Dialogs.ShowWhenFree(dialog) == ContentDialogResult.Primary)
            {
                var package = new Windows.ApplicationModel.DataTransfer.DataPackage();
                package.SetText(address);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(package);
            }
        }
        finally { _linkDialogOpen = false; }
    }

    private async void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (!_app.IsChangingThisPc) return;
        args.Cancel = true;
        // A job's log or a question may be open already, and WinUI shows one dialog at a time. That one is in front of the person, so it
        // is left alone and the window just stays open.
        await Dialogs.ShowIfFree(new ContentDialog
        {
            Title = ".Clarion is still working",
            Content = "Closing now could leave a change half done. Wait for it to finish, then close the window.",
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot,
        });
    }

    private void OnGlobalSearch(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) => OpenSearch(args.QueryText.Trim());

    private void OpenSearch(string q)
    {
        if (q.Length == 0) return;
        Nav.SelectedItem = null;
        ContentFrame.Navigate(typeof(TweakListPage), new PageArgs("Search results", $"Settings matching \"{q}\"", [], q));
    }

    public void ApplyTheme(ElementTheme theme)
    {
        Root.RequestedTheme = theme;
        ThemeBrushes.Current = Root.ActualTheme;
        UpdateCaptionButtons();
    }

    /// <summary>The minimize, maximize and close buttons draw on top of our own title bar, so they need to follow the theme.</summary>
    private void UpdateCaptionButtons()
    {
        var bar = AppWindow.TitleBar;
        // In High Contrast the person's own colors apply, so the buttons go back to the system's.
        if (new Windows.UI.ViewManagement.AccessibilitySettings().HighContrast)
        {
            bar.ButtonBackgroundColor = bar.ButtonInactiveBackgroundColor = bar.ButtonForegroundColor = bar.ButtonHoverForegroundColor = null;
            bar.ButtonHoverBackgroundColor = bar.ButtonPressedForegroundColor = bar.ButtonInactiveForegroundColor = null;
            return;
        }
        var theme = Root.ActualTheme;
        var fg = Clarion.App.Controls.ThemeBrushes.Color("ClTextPrimary", theme);
        bar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
        bar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
        bar.ButtonForegroundColor = fg;
        bar.ButtonHoverForegroundColor = fg;
        bar.ButtonHoverBackgroundColor = Clarion.App.Controls.ThemeBrushes.Color("ClBorder", theme);
        bar.ButtonPressedForegroundColor = fg;
        bar.ButtonInactiveForegroundColor = Clarion.App.Controls.ThemeBrushes.Color("ClTextMuted", theme);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    private void ResizeToFit()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var appWindow = AppWindow.GetFromWindowId(Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd));
        var area = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary).WorkArea;
        // Sized in device independent pixels (1088 x 720, which is 1360 x 900 on a 125% screen). A fixed pixel size gave a 150%
        // screen a window about 900 wide, too narrow for the pages, which are laid out for about 1050.
        var dpiScale = GetDpiForWindow(hwnd) / 96.0;
        var width = Math.Min((int)(1088 * dpiScale), area.Width - 80);
        var height = Math.Min((int)(720 * dpiScale), area.Height - 80);
        // Size alone leaves the position to Windows, which can start the window low enough to push the bottom bar off the screen.
        var x = area.X + (area.Width - width) / 2;
        var y = area.Y + (area.Height - height) / 2;
        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(x, y, width, height));

        // The pages are laid out for a window about this big. Smaller than this and the cards start to crowd.
        // The limit is in real pixels, not scaled ones (measured: asking for 980 gave 980 at 125% scaling), so it follows the screen's scale.
        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            var scale = GetDpiForWindow(hwnd) / 96.0;
            // Never ask for more than the screen has. At 200% scaling on a 1080p screen 640 scaled pixels is taller than the work area,
            // and a window that cannot be made smaller would push the bottom bar off the screen.
            presenter.PreferredMinimumWidth = Math.Min((int)(860 * scale), area.Width);
            presenter.PreferredMinimumHeight = Math.Min((int)(640 * scale), area.Height);
        }
    }

    /// <summary>The lists of settings. The first two are pages of their own, and the rest are the tabs of the Windows page.</summary>
    internal static readonly Dictionary<string, PageArgs> Sections = new()
    {
        ["privacy"] = new("Privacy", "Stop Windows from collecting and sharing more than it needs to.", ["Privacy", "Security"]),
        ["debloat"] = new("Debloat", "Remove apps you do not use. Every removal can be reverted.", ["Debloat"]),
        ["apps"] = new("Apps", "Install the programs you want with winget. Each is pinned to one exact package.", ["Install"]),
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
            _pageTag = "settings";
            ContentFrame.Navigate(typeof(SettingsPage));
            UpdateBar();
            return;
        }

        var tag = (args.SelectedItem as NavigationViewItem)?.Tag as string;
        _pageTag = tag;
        UpdateBar();
        switch (tag)
        {
            case "home": ContentFrame.Navigate(typeof(HomePage)); break;
            case "windows": ContentFrame.Navigate(typeof(WindowsPage)); break;
            case "apps": ContentFrame.Navigate(typeof(AppsPage)); break;
            case "feedback": ContentFrame.Navigate(typeof(FeedbackPage)); break;
            case "system": ContentFrame.Navigate(typeof(SystemPage)); break;
            case "troubleshoot": ContentFrame.Navigate(typeof(TroubleshootPage)); break;
            case "verify": ContentFrame.Navigate(typeof(VerifyPage)); break;
            case "whatsnew": ContentFrame.Navigate(typeof(WhatsNewPage)); break;
            case "about": ContentFrame.Navigate(typeof(AboutPage)); break;
            case "safety": ContentFrame.Navigate(typeof(SafetyPage)); break;
            case "cleanup": ContentFrame.Navigate(typeof(CleanupPage)); break;
            case "repair": ContentFrame.Navigate(typeof(RepairPage)); break;
            case not null when Sections.TryGetValue(tag, out var args2): ContentFrame.Navigate(typeof(TweakListPage), args2); break;
        }
    }

    private TaskbarProgress? _taskbar;

    private void UpdateActivity()
    {
        ShowTaskbarProgress();
        ActivityPanel.Visibility = Activity.IsOpen ? Visibility.Visible : Visibility.Collapsed;
        ActivityBar.IsIndeterminate = Activity.Indeterminate;
        ActivityBar.Maximum = Math.Max(1, Activity.Total);
        ActivityBar.Value = Activity.Value;
        ActivityHide.IsEnabled = !Activity.IsRunning;
        ActivityStop.Visibility = Activity.IsRunning && _runStop is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Mirrors the run on the taskbar button: moving while it works, yellow when stopped, red when something failed, gone when it is closed.</summary>
    private void ShowTaskbarProgress()
    {
        _taskbar ??= new TaskbarProgress(WindowNative.GetWindowHandle(this));
        if (Activity.IsRunning)
        {
            if (Activity.Indeterminate) _taskbar.Set(TaskbarState.Indeterminate);
            else _taskbar.Set(TaskbarState.Normal, Activity.Value, Activity.Total);
        }
        else if (Activity.IsOpen && Activity.EndedWithProblems) _taskbar.Set(TaskbarState.Error, 1, 1);
        else if (Activity.IsOpen && Activity.EndedStopped) _taskbar.Set(TaskbarState.Paused, 1, 1);
        else _taskbar.Set(TaskbarState.None);
    }

    private CancellationTokenSource? _runStop;

    private void OnStopRun(object sender, RoutedEventArgs e)
    {
        _runStop?.Cancel();
        // An app install can run for many minutes, so it is stopped too, with the installer winget started.
        _app.Runtime.Winget.CancelRunning();
        ActivityStop.IsEnabled = false;
        Activity.Info("Stopping. A setting in progress is finished or, for an app install, stopped. The rest stay queued.");
    }

    private void ScrollActivity()
    {
        ActivityScroll.UpdateLayout();
        ActivityScroll.ChangeView(null, ActivityScroll.ScrollableHeight, null, disableAnimation: true);
    }

    private void OnHideActivity(object sender, RoutedEventArgs e) => Activity.Close();

    private void OnOpenLog(object sender, RoutedEventArgs e) =>
        Shell.Open("notepad.exe", $"\"{Log.Path}\"");

    /// <summary>Reads the system, then welcomes someone who has never applied anything.</summary>
    private async Task StartupAsync()
    {
        await _app.RefreshStatesAsync();
        // A queue carried across the restart for administrator rights comes back first. Its owner has already made the first choice,
        // so the welcome is not shown again.
        try { if (await _app.RestoreCarriedQueueAsync()) return; }
        catch (Exception ex) { Log.Write($"The carried queue could not be restored: {ex.Message}"); }
        try { await ShowWelcomeAsync(); }
        catch (Exception ex) { Log.Write($"Welcome could not be shown: {ex.Message}"); }
    }

    /// <summary>
    /// Once, for a new person: pick a starting point, see what it would change, then apply it. Someone who already has settings applied is not
    /// welcomed again, and an old install that never saved this choice is treated the same way.
    /// </summary>
    private async Task ShowWelcomeAsync()
    {
        if (_app.Settings.FirstRunDone) return;
        if (!_app.Settings.NeedsWelcome(_app.Runtime.Journal.TweakIdsWithOutstanding().Count))
        {
            _app.UpdateSettings(s => s with { FirstRunDone = true });
            return;
        }

        var choices = new RadioButtons { SelectedIndex = 1 };
        foreach (var (_, title, text) in Clarion.Core.Catalog.PresetInfo.All)
        {
            var row = new StackPanel { Spacing = 2, Margin = new Thickness(0, 2, 0, 6) };
            row.Children.Add(new TextBlock { Text = title + (title == "Standard" ? "  (suggested)" : ""), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            row.Children.Add(Ui.Note(text, 460));
            choices.Items.Add(row);
        }

        var body = new StackPanel { Spacing = 12 };
        body.Children.Add(new TextBlock
        {
            Text = ".Clarion explains every change before it makes it, takes a restore point first, and can undo anything it did.",
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 480,
        });
        // The whole safety loop in four short lines, so the first change is not the first time a person meets Review, Verify or Revert.
        // It sits above the presets, because the presets are what scrolls in a short window.
        var loop = new StackPanel { Spacing = 2 };
        loop.Children.Add(new TextBlock { Text = "How it works", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        foreach (var line in new[]
        {
            "1. Queue. Switch settings on or off. Nothing changes yet.",
            "2. Review. You see every exact change, then press Apply.",
            "3. Verify. Later, .Clarion checks that Windows has not put things back.",
            "4. Revert. Any change can be undone from the Safety page.",
        })
            loop.Children.Add(Ui.Note(line, 460));
        body.Children.Add(loop);
        body.Children.Add(new TextBlock { Text = "Pick a starting point", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        body.Children.Add(choices);

        var dialog = new ContentDialog
        {
            Title = "Welcome to .Clarion",
            Content = new ScrollViewer { Content = body, MaxHeight = 460 },
            PrimaryButtonText = "Queue this and show me",
            SecondaryButtonText = "I will look around myself",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = Content.XamlRoot,
        };
        var result = await dialog.ShowAsync();
        _app.UpdateSettings(s => s with { FirstRunDone = true });

        if (result != ContentDialogResult.Primary) return;
        var key = Clarion.Core.Catalog.PresetInfo.All[Math.Max(0, choices.SelectedIndex)].Key;
        if (_app.QueuePreset(key) > 0) _app.RequestReview();
    }

    private string? _pageTag;

    // Pages that cannot add to the queue. Review and apply and Clear queue have nothing to do there, so they stay out of sight until
    // something is queued from another page, and then they show everywhere so the queue can still be reviewed.
    private static readonly HashSet<string> PagesWithoutQueue = ["system", "feedback", "whatsnew", "about", "cleanup", "repair", "settings"];

    private void UpdateBar()
    {
        var count = _app.PendingCount;
        var showQueue = count > 0 || _pageTag is null || !PagesWithoutQueue.Contains(_pageTag);
        ClearButton.Visibility = ReviewButton.Visibility = showQueue ? Visibility.Visible : Visibility.Collapsed;
        BusyRing.IsActive = _app.IsBusy;
        StatusText.Text = _app.IsBusy ? _app.Status
            : count == 0 ? _app.Status
            : count == 1 ? "1 change queued" : $"{count} changes queued";
        ReviewButton.IsEnabled = count > 0 && !_app.IsBusy;
        ClearButton.IsEnabled = count > 0 && !_app.IsBusy;
        RefreshButton.IsEnabled = !_app.IsBusy;
    }

    /// <summary>A dot on "What's new" while there is something to read: a newer version, or an update the person has not looked at yet.</summary>
    private void UpdateWhatsNewBadge()
    {
        WhatsNewBadge.Visibility = _app.NewerReleases.Count > 0 || _app.UpdatedFromVersion is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateVerifyBadge()
    {
        VerifyBadge.Value = _app.DriftCount;
        VerifyBadge.Visibility = _app.DriftCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    }


    private void GoTo(string tag)
    {
        // The five Windows lists are tabs of one page, so asking for one selects that page and the tab.
        var windowsTab = WindowsPage.IsTab(tag);
        if (windowsTab) { WindowsPage.Tab = tag; tag = "windows"; }

        var target = Nav.MenuItems.Concat(Nav.FooterMenuItems).OfType<NavigationViewItem>()
            .FirstOrDefault(i => i.Tag as string == tag);
        if (target is null) return;
        // Already on the page: the selection does not change, so the tab is shown directly.
        if (windowsTab && ReferenceEquals(Nav.SelectedItem, target)) ContentFrame.Navigate(typeof(WindowsPage));
        else Nav.SelectedItem = target;
    }

    private void OnClear(object sender, RoutedEventArgs e) => _app.ClearPending();

    private async void OnRefresh(object sender, RoutedEventArgs e)
    {
        if (!_app.IsBusy) await _app.RefreshStatesAsync(fresh: true);
    }

    /// <summary>A link that opens in the user's browser without Clarion's administrator rights. A NavigateUri would open it with them.</summary>
    private static HyperlinkButton MakeLink(SourceLink link)
    {
        var button = new HyperlinkButton { Content = link.Text, Padding = new Thickness(4, 0, 4, 0), FontSize = 12 };
        button.Click += (_, _) => Shell.Open(link.Link.AbsoluteUri);
        return button;
    }

    private async void OnReview(object sender, RoutedEventArgs e)
    {
        if (_app.PendingCount == 0) return;

        if (!_app.IsElevated)
        {
            // The history and settings live in a folder only administrators can write to, and a change that cannot be recorded is not made.
            var ask = new ContentDialog
            {
                Title = "Administrator rights are needed to make changes",
                Content = ".Clarion keeps its history in a folder only administrators can write to, and it will not change anything it cannot record. Restart it as administrator to continue. The queue you built comes back in the new window, and the review opens again.",
                PrimaryButtonText = "Restart as administrator",
                CloseButtonText = "Not now",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = Content.XamlRoot,
            };
            if (await ask.ShowAsync() == ContentDialogResult.Primary) ((App)Application.Current).RestartAsAdministrator();
            return;
        }

        var list = new StackPanel { Spacing = 12 };
        var installs = new List<(TweakItem Item, TextBlock Details)>();
        var rows = new List<(TweakItem Item, FrameworkElement Row, HyperlinkButton LeaveOut)>();
        var kinds = new Dictionary<TweakItem, Clarion.Core.Layout.ReviewKind>();
        foreach (var item in _app.Pending.ToList())
        {
            kinds[item] = Clarion.Core.Layout.ReviewGroups.Of(item.InstallsApps, item.RemovesApps, item.IsOn);
            var block = new StackPanel { Spacing = 2 };
            var verb = item.InstallsApps ? (item.IsOn ? "install" : "uninstall") : item.IsOn ? "turn on" : "revert";
            block.Children.Add(new TextBlock
            {
                Text = $"{item.Name}  ({verb})",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap,
            });
            if (item.IsOn)
            {
                foreach (var step in item.ExactChanges)
                    block.Children.Add(Ui.Mono(step));
                if (item.RemovesApps)
                    block.Children.Add(new TextBlock { Text = Clarion.Core.Appx.AppRemovalNotice.Text, FontSize = 12, TextWrapping = TextWrapping.Wrap, Foreground = Clarion.App.Controls.ThemeBrushes.Get("ClWarnText", Root.ActualTheme) });
                if (item.InstallsApps)
                {
                    // What winget itself says about this package, read when the dialog opens, so the person sees who serves the installer
                    // and the checksum winget will hold it to before they accept anything.
                    var details = new TextBlock { Text = "Asking winget for the version, licence and checksum...", FontSize = 12, TextWrapping = TextWrapping.Wrap, Opacity = 0.8, IsTextSelectionEnabled = true };
                    block.Children.Add(details);
                    var links = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 4 };
                    var shown = item.SourceLinks.Where(l => !l.Link.AbsoluteUri.Contains("winget-pkgs", StringComparison.Ordinal))

                        // The same page is often listed once as http and once as https, so the scheme is left out of the comparison.
                        .DistinctBy(l => (l.Link.Host + l.Link.PathAndQuery).TrimEnd('/').ToLowerInvariant()).ToList();
                    // Some records name no homepage or licence page. The package record on GitHub is then the one place to read from.
                    if (shown.Count == 0) shown = item.SourceLinks.Take(1).ToList();
                    foreach (var link in shown) links.Children.Add(MakeLink(link));
                    block.Children.Add(links);
                    installs.Add((item, details));
                    if (item.SupportsUserScope) AddUserScopeChoice(block, item);
                }
            }
            else if (item.InstallsApps)
            {
                block.Children.Add(Ui.Note("Runs the program's own uninstaller through winget. If you did not install it with .Clarion, this removes the copy you already have."));
            }
            else
            {
                // The steps Revert will run, read from the history exactly as the engine reads it, so nothing is a surprise.
                var preview = Clarion.Core.Reports.RevertPreview.For(_app.Runtime.Journal.OutstandingFor(item.Id), item.Tweak.Undo);
                foreach (var step in preview.Lines)
                    block.Children.Add(Ui.Mono(step));
                block.Children.Add(Ui.Note(preview.FromHistory ? "Puts back the value that was recorded before the change." : "Nothing was recorded for this one, so the default shown is used."));
            }

            // One item can be taken out here without clearing the rest of the queue.
            var leaveOut = new HyperlinkButton { Content = "Leave out", VerticalAlignment = VerticalAlignment.Top, Padding = new Thickness(8, 2, 8, 2), FontSize = 12 };
            Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(leaveOut, $"Leave out {item.Name}");
            var row = Ui.StarAutoRow(8);
            row.Children.Add(block);
            Grid.SetColumn(leaveOut, 1);
            row.Children.Add(leaveOut);
            rows.Add((item, row, leaveOut));
        }

        // A long queue is sorted into sections, each with its own count, so settings, installs and removals can be read one kind at a time.
        var sections = new List<(Clarion.Core.Layout.ReviewKind Kind, Expander Box)>();
        if (Clarion.Core.Layout.ReviewGroups.ShouldGroup(kinds.Values))
        {
            foreach (var kind in Clarion.Core.Layout.ReviewGroups.Order.Where(k => kinds.ContainsValue(k)))
            {
                var inside = new StackPanel { Spacing = 12 };
                foreach (var (_, row, _) in rows.Where(r => kinds[r.Item] == kind)) inside.Children.Add(row);
                var box = new Expander
                {
                    IsExpanded = true,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Header = new TextBlock { FontWeight = Microsoft.UI.Text.FontWeights.SemiBold },
                    Content = inside,
                };
                sections.Add((kind, box));
                list.Children.Add(box);
            }
        }
        else
        {
            foreach (var (_, row, _) in rows) list.Children.Add(row);
        }

        void RefreshSections()
        {
            foreach (var (kind, box) in sections)
            {
                var count = rows.Count(r => kinds[r.Item] == kind && r.Row.Visibility == Visibility.Visible);
                var title = Clarion.Core.Layout.ReviewGroups.Title(kind, count);
                ((TextBlock)box.Header).Text = title;
                Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(box, title);
                box.Visibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        RefreshSections();

        // A restore point before a few installs costs minutes, so it starts unticked when nothing in the queue changes Windows settings.
        var onlyApps = _app.Pending.All(p => p.InstallsApps);
        var restore = new CheckBox { Content = onlyApps ? "Create a restore point first" : "Create a restore point first (recommended)", IsChecked = !onlyApps };
        var body = new StackPanel { Spacing = 14 };
        body.Children.Add(new ScrollViewer { MaxHeight = 380, Content = list });

        // The licence terms are the publishers', and they are accepted here by the person, never on their behalf.
        CheckBox? agree = null;
        if (installs.Count > 0)
        {
            // Off until winget has shown what is being agreed to, so nothing can be accepted while every row still says it is asking.
            agree = new CheckBox { Content = "I have read the licence terms of the apps being installed and I accept them", IsChecked = false, IsEnabled = false };
            ToolTipService.SetToolTip(agree, "Available when winget has shown the details above.");
            body.Children.Add(agree);
        }
        body.Children.Add(restore);

        var dialog = new ContentDialog
        {
            Title = $"Apply {_app.PendingCount} change{(_app.PendingCount == 1 ? "" : "s")}?",
            Content = body,
            PrimaryButtonText = "Apply",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = agree is null,
            XamlRoot = Content.XamlRoot,
        };
        if (agree is not null)
        {
            agree.Checked += (_, _) => dialog.IsPrimaryButtonEnabled = true;
            agree.Unchecked += (_, _) => dialog.IsPrimaryButtonEnabled = false;
        }

        foreach (var (item, row, leaveOut) in rows)
        {
            leaveOut.Click += (_, _) =>
            {
                // Back to what is on this PC now, which is what Clear queue does for every item.
                item.IsOn = item.IsApplied;
                item.SetUserScope(false);
                row.Visibility = Visibility.Collapsed;
                RefreshSections();
                var left = rows.Where(r => r.Row.Visibility == Visibility.Visible).ToList();
                if (left.Count == 0) { dialog.Hide(); return; }
                dialog.Title = $"Apply {left.Count} change{(left.Count == 1 ? "" : "s")}?";
                if (agree is not null && left.All(r => !r.Item.InstallsApps || !r.Item.IsOn))
                {
                    // Nothing left to install, so there are no terms to accept.
                    agree.IsChecked = false;
                    agree.Visibility = Visibility.Collapsed;
                    dialog.IsPrimaryButtonEnabled = true;
                }
            };
        }

        using var stopReading = new CancellationTokenSource();
        if (installs.Count > 0) _ = ReadAppDetailsAsync(installs, agree!, stopReading.Token);
        var answer = await dialog.ShowAsync();
        stopReading.Cancel();
        // Installing for one account only is a choice for this run, so it starts unticked again the next time.
        if (answer != ContentDialogResult.Primary || _app.PendingCount == 0)
        {
            foreach (var (item, _, _) in rows) item.SetUserScope(false);
            return;
        }

        // Consent holds for this run only, and only while an app install is still in the queue.
        var stillInstalling = _app.Pending.Any(p => p.InstallsApps && p.IsOn);
        _app.Runtime.Winget.AcceptPackageAgreements = stillInstalling && agree?.IsChecked == true;
        try { await RunAsync(restore.IsChecked == true); }
        finally
        {
            _app.Runtime.Winget.AcceptPackageAgreements = false;
            foreach (var (item, _, _) in rows) item.SetUserScope(false);
        }
    }

    /// <summary>
    /// The review's choice to install an app for the signed in account only. It says in plain words what that means and why it is offered for this app,
    /// since scope is not consistent across installers.
    /// </summary>
    private static void AddUserScopeChoice(StackPanel block, TweakItem item)
    {
        var forMe = new CheckBox { Content = "Install for this account only", IsChecked = item.UserScope };
        forMe.Checked += (_, _) => item.SetUserScope(true);
        forMe.Unchecked += (_, _) => item.SetUserScope(false);
        block.Children.Add(forMe);

        var basis = Clarion.Core.Apps.AppScopes.Default.For(item.Id)?.Basis switch
        {
            "declared" => "Its winget package record says it can be installed for one account.",
            "portable" => "winget itself carries this out for a portable app.",
            "msix" => "A packaged app installs for the account that installs it.",
            _ => "",
        };
        block.Children.Add(new TextBlock
        {
            Text = $"It goes in your own profile and not in Program Files, so other accounts on this PC do not get it, and it is removed from your account too. {basis} Not every installer follows this, so look where it went. Leave it off to install for everyone, as before.",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Opacity = 0.8,
        });
    }

    // The details of an install are read from winget while the review is open, a few at a time. The tick box that accepts the licence
    // terms stays locked until every row has its details or a note that they could not be read, so one slow answer must not hold it
    // for long: the whole read has a time limit, and a row that is still waiting then says so.
    private static readonly TimeSpan DetailsBudget = TimeSpan.FromSeconds(60);
    private const int DetailsInParallel = 4;

    /// <summary>Fills in each install's details from winget while the review dialog is open.</summary>
    private async Task ReadAppDetailsAsync(List<(TweakItem Item, TextBlock Details)> installs, CheckBox agree, CancellationToken stop)
    {
        using var budget = CancellationTokenSource.CreateLinkedTokenSource(stop);
        budget.CancelAfter(DetailsBudget);
        using var gate = new SemaphoreSlim(DetailsInParallel);

        var rows = installs.Select(async row =>
        {
            var (item, details) = row;
            var id = item.Tweak.Apply.OfType<Clarion.Core.Model.InstallWingetPackage>().Select(o => o.Id).FirstOrDefault();
            Clarion.Core.Apps.WingetPackageInfo? info = null;
            var tookTooLong = false;
            try
            {
                await gate.WaitAsync(budget.Token);
                try
                {
                    var work = Task.Run(() => id is null ? null : _app.Runtime.Winget.Show(id));
                    // A winget call that does not come back is left to end by itself. Its answer is no longer wanted.
                    var first = await Task.WhenAny(work, Task.Delay(Timeout.Infinite, budget.Token));
                    if (ReferenceEquals(first, work)) info = await work;
                    else tookTooLong = true;
                }
                finally { gate.Release(); }
            }
            catch (OperationCanceledException) { tookTooLong = true; }
            catch (Exception ex) when (ex is not OutOfMemoryException) { Log.Write($"winget could not show {id}: {ex.Message}"); }
            if (stop.IsCancellationRequested) return;
            details.Text = info is null
                ? (tookTooLong ? "winget took too long to answer for this package. " : "winget could not read this package's details just now. ")
                  + "Installing still checks the installer's checksum, but you cannot see it here first."
                : $"Version {info.Version}, from {info.Publisher}\nLicence: {(info.License.Length > 0 ? info.License : "see the link below")}\nInstaller from {info.InstallerHost} ({info.InstallerType})\nChecksum (SHA-256): {info.Sha256}\nwinget refuses to run the installer if its checksum differs.";
            details.Opacity = 1;
        }).ToList();

        await Task.WhenAll(rows);
        if (stop.IsCancellationRequested) return;
        // Every row has either its details or the note that they could not be read, so the terms can be accepted now.
        agree.IsEnabled = true;
        ToolTipService.SetToolTip(agree, null);
    }

    private async Task RunAsync(bool restorePoint, bool turnOnProtection = false)
    {
        var restartsExplorer = _app.Pending.Any(p => p.Tweak.RestartsExplorer);
        var needsReboot = _app.Pending.Any(p => p.Tweak.NeedsReboot);
        var needsSignOut = _app.Pending.Any(p => p.Tweak.NeedsSignOut);

        Activity.Start(restorePoint ? "Applying changes, making a restore point first" : "Applying changes");
        using var stop = new CancellationTokenSource();
        _runStop = stop;
        ActivityStop.IsEnabled = true;
        UpdateActivity();
        Clarion.Core.Engine.BatchResult result;
        try
        {
            result = await _app.ApplyPendingAsync(restorePoint,
                msg => DispatcherQueue.TryEnqueue(() => { _app.Status = msg; Activity.Info(msg); }),
                step => DispatcherQueue.TryEnqueue(() => Activity.Step(step)),
                turnOnProtection, stop.Token);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            // Any failure ends here, not only the few kinds expected. Otherwise the activity panel would stay open and the Stop button live.
            Log.Write($"The run did not finish: {ex}");
            Activity.Close();
            await Dialogs.Say(Content.XamlRoot, "The run did not finish", $"Some changes may have been made. .Clarion has read your settings again, so what you see is what Windows has now. {ex.Message}");
            return;
        }
        finally { _runStop = null; }
        if (result.Blocked is not null) Activity.Close();
        else Activity.Finish(result.Items.Count(i => i.Result.Success && !i.Skipped), result.Items.Count(i => !i.Result.Success), result.Items.Count(i => i.Skipped && i.Result.Success), result.NotStarted);

        if (result.Blocked is not null)
        {
            var retry = new ContentDialog
            {
                Title = "No restore point, nothing was changed",
                Content = result.Blocked + (turnOnProtection
                    ? "\n\nSystem Protection could not be turned on, or the restore point still failed. A policy on this PC can block it."
                    : "\n\nSystem Protection is often off on a new Windows install. .Clarion can turn it on for your system drive and try again. Restore points keep some disk space."),
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

        // An installer can ask for a restart even though the setting itself does not need one.
        needsReboot |= _app.Runtime.Winget.TakeRestartNeeded();
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
            body.Children.Add(new TextBlock { Text = "Explorer needs a restart to show some changes. Restarting it closes every open folder window, interrupts a file copy running in one, and makes the taskbar and desktop icons disappear for a moment.", TextWrapping = TextWrapping.Wrap });
            dialog.PrimaryButtonText = "Restart Explorer now";
        }
        if (await dialog.ShowAsync() == ContentDialogResult.Primary) RestartExplorer();
    }

    private static void RestartExplorer()
    {
        // Only the Explorer of this sign in. With another account signed in at the same time, its Explorer is not ours to close.
        using var self = Process.GetCurrentProcess();
        var session = self.SessionId;
        foreach (var p in Process.GetProcessesByName("explorer"))
        {
            using (p)
            {
                try { if (p.SessionId == session) p.Kill(); } catch (InvalidOperationException) { } catch (System.ComponentModel.Win32Exception) { }
            }
        }
    }
}
