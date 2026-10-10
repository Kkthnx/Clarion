using Clarion.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Pages;

/// <summary>The About page: why Clarion exists, who makes it, and how to support it.</summary>
public sealed partial class AboutPage : Page
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private void OnWebsite(object sender, RoutedEventArgs e) => Shell.Open("https://kkthnx.com");
    private void OnGitHub(object sender, RoutedEventArgs e) => Shell.Open("https://github.com/Kkthnx");
    private void OnSource(object sender, RoutedEventArgs e) => Shell.Open("https://github.com/Kkthnx/Clarion");
    private void OnTwitch(object sender, RoutedEventArgs e) => Shell.Open("https://twitch.tv/kkthnxtv");
    private void OnYouTube(object sender, RoutedEventArgs e) => Shell.Open("https://www.youtube.com/@KKTHNXTV");
    private void OnX(object sender, RoutedEventArgs e) => Shell.Open("https://x.com/KkthnxUI");
    private void OnFacebook(object sender, RoutedEventArgs e) => Shell.Open("https://www.facebook.com/KkthnxUI");
    private void OnPayPal(object sender, RoutedEventArgs e) => Shell.Open("https://www.paypal.me/kkthnxTV");
    private void OnPatreon(object sender, RoutedEventArgs e) => Shell.Open("https://patreon.com/kkthnx");
}
