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

    private static string Version => AppServices.Version;

    private void OnOpenLog(object sender, RoutedEventArgs e)
    {
        if (!File.Exists(Log.Path)) Log.Write("Log opened");
        Shell.Open(Log.Path);
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
