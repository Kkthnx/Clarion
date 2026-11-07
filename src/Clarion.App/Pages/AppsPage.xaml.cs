using Clarion.App.Controls;
using Clarion.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

/// <summary>Installs programs with winget. The list is the shared settings list, with a banner above it about winget.</summary>
public sealed partial class AppsPage : Page
{
    private const string WingetDocs = "https://learn.microsoft.com/en-us/windows/package-manager/winget/install";

    private readonly AppServices _app = AppServices.Instance;

    public AppsPage()
    {
        InitializeComponent();
        // A new page is made on every visit, so the handler on the app-wide object goes on when the page shows and off when it is left.
        Loaded += (_, _) =>
        {
            if (_attached || _released) return;
            _attached = true;
            _app.PropertyChanged += OnAppChanged;
        };
        Unloaded += (_, _) => Detach();
    }

    private bool _attached;
    private bool _released;

    private void Detach()
    {
        if (_attached)
        {
            _attached = false;
            _app.PropertyChanged -= OnAppChanged;
        }
        if (_released) return;
        _released = true;
        PageRelease.Lists(this);
    }

    // Unloaded is not raised for a page that was measured and then left before it was shown, and its rows are already bound. This is.
    protected override void OnNavigatedFrom(NavigationEventArgs e) => Detach();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Inner.Navigate(typeof(TweakListPage), MainWindow.Sections["apps"]);
        Inner.BackStack.Clear();
        ShowBanner();
        // Which apps are installed is read now, the first time this page opens, and not when Clarion starts.
        _ = _app.EnsureAppsReadAsync();
    }

    private void OnOpenRepair(object sender, RoutedEventArgs e) => _app.RequestNavigate("repair");

    private void OnAppChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => DispatcherQueue.TryEnqueue(() => { if (_attached) ShowBanner(); });

    private void ShowBanner()
    {
        UpdatesButton.IsEnabled = _app.Runtime.Winget.IsAvailable && _app.AppsProblem is null;
        if (!_app.Runtime.Winget.IsAvailable)
        {
            WingetBar.Severity = InfoBarSeverity.Warning;
            WingetBar.Title = "winget was not found for this account";
            WingetBar.Message = "Apps are installed with winget, Microsoft's package manager, which comes with App Installer. It is not at its usual place for this account, so the apps below are dimmed. If App Installer is on this PC, the Repair page has a job that registers it again. If it is not, install it from the Microsoft Store. Then press Re-check. .Clarion does not install it for you.";
            WingetLink.Visibility = Visibility.Visible;
            RepairLink.Visibility = Visibility.Visible;
            WingetBar.IsClosable = false;
            WingetBar.IsOpen = true;
            WingetBar.Visibility = Visibility.Visible;
            return;
        }

        if (_app.AppsProblem is { } problem)
        {
            WingetBar.Severity = InfoBarSeverity.Warning;
            WingetBar.Title = "Apps cannot be used right now";
            WingetBar.Message = problem;
            WingetBar.IsClosable = false;
            WingetLink.Visibility = Visibility.Collapsed;
            RepairLink.Visibility = Visibility.Collapsed;
            WingetBar.IsOpen = true;
            WingetBar.Visibility = Visibility.Visible;
            return;
        }

        // The note about winget's source terms stays closed once it has been, since it is the same every time.
        if (_app.Settings.AppsNoticeClosed)
        {
            WingetBar.IsOpen = false;
            WingetBar.Visibility = Visibility.Collapsed;
            return;
        }
        WingetBar.Severity = InfoBarSeverity.Informational;
        WingetBar.Title = "Installed with winget, one exact package at a time";
        WingetBar.Message = "The review shows who serves each installer and its checksum, and nothing installs until you accept the licence terms yourself. winget stalls unless its own source terms are accepted, so .Clarion passes that for you.";
        WingetLink.Visibility = Visibility.Collapsed;
        WingetBar.IsClosable = true;
        WingetBar.IsOpen = true;
        WingetBar.Visibility = Visibility.Visible;
    }

    private async void OnCheckUpdates(object sender, RoutedEventArgs e)
    {
        UpdatesButton.IsEnabled = false;
        try { await AppUpdatesFlow.RunAsync(XamlRoot); }
        finally { UpdatesButton.IsEnabled = _app.Runtime.Winget.IsAvailable && _app.AppsProblem is null; }
    }

    private void OnNoticeClosed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        if (args.Reason == InfoBarCloseReason.CloseButton) _app.UpdateSettings(s => s with { AppsNoticeClosed = true });
        WingetBar.Visibility = Visibility.Collapsed;
    }

    private void OnWingetLink(object sender, RoutedEventArgs e) => Shell.Open(WingetDocs);
}
