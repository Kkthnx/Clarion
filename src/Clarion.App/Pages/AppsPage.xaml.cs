using Clarion.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

/// <summary>Installs programs with winget. The list is the shared settings list, with a banner above it about winget.</summary>
public sealed partial class AppsPage : Page
{
    private const string WingetDocs = "https://learn.microsoft.com/en-us/windows/package-manager/winget/install";

    // The notice about winget's source terms is shown until it is closed, then not again during this run.
    private static bool _noticeClosed;

    private readonly AppServices _app = AppServices.Instance;

    public AppsPage()
    {
        InitializeComponent();
        _app.PropertyChanged += OnAppChanged;
        // A new page is made on every visit, so the handler on the app-wide object must go when the page does.
        Unloaded += (_, _) => _app.PropertyChanged -= OnAppChanged;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        Inner.Navigate(typeof(TweakListPage), MainWindow.Sections["apps"]);
        Inner.BackStack.Clear();
        ShowBanner();
        // Which apps are installed is read now, the first time this page opens, and not when Clarion starts.
        _ = _app.EnsureAppsReadAsync();
    }

    private void OnAppChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) => DispatcherQueue.TryEnqueue(ShowBanner);

    private void ShowBanner()
    {
        if (!_app.Runtime.Winget.IsAvailable)
        {
            WingetBar.Severity = InfoBarSeverity.Warning;
            WingetBar.Title = "winget was not found for this account";
            WingetBar.Message = "Apps are installed with winget, Microsoft's package manager, which comes with App Installer. It is not at its usual place for this account, so the apps below are dimmed. Install or repair App Installer from Microsoft, then press Re-check. .Clarion does not install it for you.";
            WingetLink.Visibility = Visibility.Visible;
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
            WingetBar.IsOpen = true;
            WingetBar.Visibility = Visibility.Visible;
            return;
        }

        if (_noticeClosed)
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

    private void OnNoticeClosed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        if (args.Reason == InfoBarCloseReason.CloseButton) _noticeClosed = true;
        WingetBar.Visibility = Visibility.Collapsed;
    }

    private void OnWingetLink(object sender, RoutedEventArgs e) => Shell.Open(WingetDocs);
}
