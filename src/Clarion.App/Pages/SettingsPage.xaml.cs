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
        AutoUpdateSwitch.IsOn = _app.Settings.CheckForUpdates;
        _loading = false;
        _ = ShowMonthlyStateAsync();
    }

    private static string Version => AppServices.Version;

    /// <summary>The switch shows whether the task really exists, not just what was saved, since it can be deleted in Task Scheduler.</summary>
    private async Task ShowMonthlyStateAsync()
    {
        bool on;
        try { on = await Task.Run(_app.MonthlySchedule.IsEnabled); }
        catch (Exception ex) when (ex is InvalidOperationException or IOException) { return; }
        _loading = true;
        MonthlySwitch.IsOn = on;
        _loading = false;
        if (on != _app.Settings.MonthlyVerify) _app.UpdateSettings(s => s with { MonthlyVerify = on });
    }

    private async void OnMonthlyToggled(object sender, RoutedEventArgs e)
    {
        if (_loading) return;
        var wantOn = MonthlySwitch.IsOn;
        MonthlySwitch.IsEnabled = false;
        try
        {
            var exe = Environment.ProcessPath ?? throw new InvalidOperationException("Clarion could not find its own program file.");
            var user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            await Task.Run(() => { if (wantOn) _app.MonthlySchedule.Enable(exe, user, DateTime.Today); else _app.MonthlySchedule.Disable(); });
            _app.UpdateSettings(s => s with { MonthlyVerify = wantOn });
            MonthlyStatus.Text = wantOn
                ? "On. The first check is the next second Wednesday of the month. It only looks and changes nothing."
                : "Off. The scheduled task was removed.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or IOException)
        {
            Log.Write($"Monthly check: {ex.Message}");
            _loading = true;
            MonthlySwitch.IsOn = !wantOn;
            _loading = false;
            MonthlyStatus.Text = $"Could not change the monthly check. {ex.Message}";
        }
        finally
        {
            MonthlySwitch.IsEnabled = true;
        }
    }

    private async void OnCheckUpdate(object sender, RoutedEventArgs e)
    {
        CheckUpdateButton.IsEnabled = false;
        UpdateBusy.IsActive = true;
        UpdateStatus.Text = "Asking GitHub";
        UpdateStatus.Text = await _app.CheckForUpdateAsync(manual: true);
        UpdateBusy.IsActive = false;
        CheckUpdateButton.IsEnabled = true;
    }

    private void OnAutoUpdateToggled(object sender, RoutedEventArgs e)
    {
        if (!_loading) _app.UpdateSettings(s => s with { CheckForUpdates = AutoUpdateSwitch.IsOn });
    }

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
