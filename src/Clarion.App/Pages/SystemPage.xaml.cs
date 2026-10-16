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
        SaveButton.IsEnabled = false;
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
        InstallCard.BorderBrush = ThemeBrushes.Get(report.Install.Level == InstallLevel.Standard ? "ClBorder" : "ClWarn", ActualTheme);
        Ring.IsActive = false;
        StatusLabel.Text = "";
        RefreshButton.IsEnabled = true;
        CopyButton.IsEnabled = true;
        SaveButton.IsEnabled = true;
    }

    private async void OnRefresh(object sender, RoutedEventArgs e)
    {
        await LoadAsync(true);
    }

    /// <summary>The report as text for sharing. The computer name, user name, network addresses and account IDs are taken out first.</summary>
    private string? ShareableText() =>
        _report is null ? null : Clarion.Core.Feedback.Sanitizer.Clean(_report.ToText(), Environment.UserName, Environment.MachineName);

    private void OnCopy(object sender, RoutedEventArgs e)
    {
        if (ShareableText() is not { } text) return;
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
        StatusLabel.Text = "Copied, with the computer name, user name and network addresses removed.";
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (ShareableText() is not { } text) return;
        var path = FileDialogs.Save(((App)Application.Current).WindowHandle, "Save the system report", "Text file (*.txt)|*.txt", "clarion-system-report.txt");
        if (path is null) return;
        try
        {
            File.WriteAllText(path, text);
            StatusLabel.Text = $"Saved to {path}, with the computer name, user name and network addresses removed.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusLabel.Text = $"Could not save the file: {ex.Message}";
        }
    }
}

/// <summary>One line in the Windows Update card: a mark, its colour, and the sentence.</summary>
public sealed record UpdateRow(string Mark, Microsoft.UI.Xaml.Media.Brush MarkBrush, string Text);
