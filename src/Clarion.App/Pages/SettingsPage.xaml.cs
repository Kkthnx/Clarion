using System.Diagnostics;
using System.Reflection;
using Clarion.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed partial class SettingsPage : Page
{
    private readonly AppServices _app = AppServices.Instance;
    private bool _loading;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _loading = true;
        ExpertSwitch.IsOn = _app.ExpertMode;
        ThemeBox.SelectedIndex = ((App)Application.Current).Theme switch
        {
            ElementTheme.Dark => 1,
            ElementTheme.Light => 2,
            _ => 0,
        };
        VersionText.Text = $"Clarion {Version}";
        _loading = false;
    }

    private static string Version =>
        (Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.1.0").Split('+')[0];

    private void OnOpenLog(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(Log.Path)) Log.Write("Log opened");
        Process.Start(new ProcessStartInfo(Log.Path) { UseShellExecute = true });
    }

    private void OnCopySupport(object sender, RoutedEventArgs e)
    {
        var tail = File.Exists(Log.Path) ? string.Join(Environment.NewLine, File.ReadLines(Log.Path).TakeLast(40)) : "";
        var text = $"Clarion {Version}{Environment.NewLine}Windows build {_app.Profile.Build}, {_app.Profile.Edition}{Environment.NewLine}" +
                   $"Administrator: {_app.IsElevated}{Environment.NewLine}Settings in effect: {_app.AppliedCount}{Environment.NewLine}{Environment.NewLine}{tail}";
        var package = new Windows.ApplicationModel.DataTransfer.DataPackage();
        package.SetText(text);
        Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(package);
    }

    private void OnExpertToggled(object sender, RoutedEventArgs e)
    {
        if (!_loading) _app.ExpertMode = ExpertSwitch.IsOn;
    }

    private void OnThemeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading || ThemeBox.SelectedItem is not ComboBoxItem { Tag: string tag }) return;
        ((App)Application.Current).SetTheme(Enum.Parse<ElementTheme>(tag));
    }
}
