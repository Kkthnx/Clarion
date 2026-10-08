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
        _loading = false;
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
