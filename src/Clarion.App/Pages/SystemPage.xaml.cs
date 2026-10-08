using Clarion.App.Services;
using Clarion.Core.SystemInfo;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;

namespace Clarion.App.Pages;

public sealed partial class SystemPage : Page
{
    private static SystemReport? _cached;
    private readonly AppServices _app = AppServices.Instance;
    private SystemReport? _report;

    public SystemPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_cached is not null) Show(_cached);
        else await LoadAsync();
    }

    private async Task LoadAsync()
    {
        RefreshButton.IsEnabled = false;
        CopyButton.IsEnabled = false;
        Ring.IsActive = true;
        StatusLabel.Text = "Reading your system, this takes a few seconds.";
        try
        {
            var report = await Task.Run(_app.Runtime.System.Read);
            _cached = report;
            Show(report);
        }
        catch (Exception ex)
        {
            Log.Write($"system report failed: {ex.Message}");
            Ring.IsActive = false;
            StatusLabel.Text = "Could not read the system information.";
            RefreshButton.IsEnabled = true;
        }
    }

    private void Show(SystemReport report)
    {
        _report = report;
        Headline.Text = report.Install.Headline;
        Explanation.Text = report.Install.Explanation;
        Signals.ItemsSource = report.Install.Signals;
        Groups.ItemsSource = report.Groups;
        InstallCard.Visibility = Visibility.Visible;
        InstallCard.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources[
            report.Install.Level == InstallLevel.Standard ? "ClBorderBrush" : "ClWarnBrush"];
        Ring.IsActive = false;
        StatusLabel.Text = "";
        RefreshButton.IsEnabled = true;
        CopyButton.IsEnabled = true;
    }

    private async void OnRefresh(object sender, RoutedEventArgs e)
    {
        _cached = null;
        await LoadAsync();
    }

    private void OnCopy(object sender, RoutedEventArgs e)
    {
        if (_report is null) return;
        var package = new DataPackage();
        package.SetText(_report.ToText());
        Clipboard.SetContent(package);
        StatusLabel.Text = "Copied.";
    }
}
