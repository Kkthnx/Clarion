using Clarion.App.Controls;
using Clarion.App.Services;
using Clarion.Core.SystemInfo;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;

namespace Clarion.App.Pages;

public sealed partial class SystemPage : Page
{
    private readonly AppServices _app = AppServices.Instance;
    private SystemReport? _report;

    public SystemPage()
    {
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (_app.CachedSystemReport is { } cached) Show(cached);
        else await LoadAsync(false);
    }

    private async Task LoadAsync(bool refresh)
    {
        RefreshButton.IsEnabled = false;
        CopyButton.IsEnabled = false;
        Ring.IsActive = true;
        StatusLabel.Text = "Reading your system, this takes a few seconds.";
        try
        {
            Show(await _app.GetSystemReportAsync(refresh));
        }
        catch (Exception ex)
        {
            Log.Write($"system report failed: {ex.Message}");
            Ring.IsActive = false;
            StatusLabel.Text = "Could not read the system information.";
            RefreshButton.IsEnabled = true;
        }
    }

    private void ShowUpdates(UpdateVerdict? verdict)
    {
        UpdateCard.Visibility = verdict is null ? Visibility.Collapsed : Visibility.Visible;
        if (verdict is null) return;

        UpdateHeadline.Text = verdict.Headline;
        UpdateExplanation.Text = verdict.Explanation;
        UpdateFindings.ItemsSource = verdict.Findings.Select(f => f.Kind switch
        {
            UpdateFindingKind.Good => new UpdateRow("✓", ThemeBrushes.Get("ClOkText", ActualTheme), f.Text),
            UpdateFindingKind.Problem => new UpdateRow("✕", ThemeBrushes.Get("ClDangerText", ActualTheme), f.Text),
            UpdateFindingKind.Limit => new UpdateRow("!", ThemeBrushes.Get("ClWarnText", ActualTheme), f.Text),
            _ => new UpdateRow("•", ThemeBrushes.Get("ClAccentText", ActualTheme), f.Text),
        }).ToList();
        UpdateCard.BorderBrush = ThemeBrushes.Get(verdict.Level switch
        {
            UpdateLevel.Held => "ClDanger",
            UpdateLevel.Limited => "ClWarn",
            _ => "ClBorder",
        }, ActualTheme);
    }

    private void Show(SystemReport report)
    {
        _report = report;
        Headline.Text = report.Install.Headline;
        Explanation.Text = report.Install.Explanation;
        Signals.ItemsSource = report.Install.Signals;
        Groups.ItemsSource = report.Groups;
        ShowUpdates(report.Updates);
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
        await LoadAsync(true);
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

/// <summary>One line in the Windows Update card: a mark, its colour, and the sentence.</summary>
public sealed record UpdateRow(string Mark, Microsoft.UI.Xaml.Media.Brush MarkBrush, string Text);
