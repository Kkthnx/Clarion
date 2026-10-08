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

    private async void OnSaveSetup(object sender, RoutedEventArgs e)
    {
        var ids = _app.AppliedIds();
        var path = FileDialogs.Save(((App)Application.Current).WindowHandle, "Save my setup", "Clarion setup (*.json)|*.json", "my-clarion-setup.json");
        if (path is null) return;
        try
        {
            File.WriteAllText(path, Clarion.Core.Profiles.ProfileFile.Serialize(ids, Version));
            await Say("Setup saved", $"{ids.Count} settings were saved to {path}.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            await Say("Could not save the file", ex.Message);
        }
    }

    private async void OnLoadSetup(object sender, RoutedEventArgs e)
    {
        var path = FileDialogs.Open(((App)Application.Current).WindowHandle, "Load a setup", "Clarion setup (*.json)|*.json");
        if (path is null) return;
        var load = Clarion.Core.Profiles.ProfileFile.Load(path);
        if (load.Profile is null) { await Say("That file cannot be used", load.Error ?? "Unknown problem."); return; }

        var (queued, unknown, notAvailable) = _app.QueueIds(load.Profile.Tweaks);
        var text = $"{queued} settings were queued. Review them with Review and apply at the bottom.";
        if (notAvailable > 0) text += $" {notAvailable} are not available on this PC.";
        if (unknown.Count > 0) text += $" {unknown.Count} are not known to this version of Clarion.";
        await Say("Setup loaded", text);
    }

    private Task Say(string title, string text) => Dialogs.Say(XamlRoot, title, text);

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
