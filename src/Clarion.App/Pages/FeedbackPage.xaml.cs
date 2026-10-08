using System.Diagnostics;
using Clarion.App.Services;
using Clarion.Core.Feedback;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;

namespace Clarion.App.Pages;

public sealed partial class FeedbackPage : Page
{
    private const int LogLines = 40;

    private readonly AppServices _app = AppServices.Instance;
    private string _body = "";
    private string _title = "";
    private bool _ready;

    public FeedbackPage()
    {
        InitializeComponent();
        _ready = true;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        Rebuild();
        if (_app.CachedSystemReport is not null) return;
        try
        {
            await _app.GetSystemReportAsync();
            Rebuild();
        }
        catch (Exception ex)
        {
            Log.Write($"system report failed: {ex.Message}");
        }
    }

    private FeedbackKind Kind => KindBox.SelectedIndex == 1 ? FeedbackKind.Suggestion : FeedbackKind.Bug;

    private void OnChanged(object sender, RoutedEventArgs e) => Rebuild();
    private void OnKindChanged(object sender, SelectionChangedEventArgs e) => Rebuild();
    private void OnTextChanged(object sender, TextChangedEventArgs e) => Rebuild();

    private void Rebuild()
    {
        if (!_ready) return;
        var includeSystem = SystemBox.IsChecked == true;
        var input = new FeedbackInput(
            Kind, TitleBox.Text, DescriptionBox.Text, AppServices.Version,
            includeSystem, LogBox.IsChecked == true, _app.CachedSystemReport,
            _app.Runtime.Journal.TweakIdsWithOutstanding().ToList(),
            LogBox.IsChecked == true ? Log.Tail(LogLines) : []);

        var user = Environment.UserName;
        var machine = Environment.MachineName;
        _title = FeedbackReport.CleanTitle(input, user, machine);
        _body = FeedbackReport.Build(input, user, machine);
        var loading = includeSystem && _app.CachedSystemReport is null ? "\n\n(Windows and hardware details are still loading and will appear here.)" : "";
        PreviewBox.Text = $"Title: {_title}\n\n{_body}{loading}";
        OpenButton.IsEnabled = TitleBox.Text.Trim().Length > 0 || DescriptionBox.Text.Trim().Length > 0;
    }

    private void OnOpen(object sender, RoutedEventArgs e)
    {
        var (url, needsPaste) = FeedbackReport.IssueUrl(Kind, _title, _body);
        if (needsPaste) CopyToClipboard(_body);
        try
        {
            Process.Start(new ProcessStartInfo(url.AbsoluteUri) { UseShellExecute = true });
            StatusLabel.Text = needsPaste ? "Opened. The full report is on your clipboard, paste it into the box." : "Opened in your browser.";
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            CopyToClipboard(_body);
            StatusLabel.Text = "Could not open the browser. The report is on your clipboard.";
        }
    }

    private void OnCopy(object sender, RoutedEventArgs e)
    {
        CopyToClipboard($"{_title}\n\n{_body}");
        StatusLabel.Text = "Copied.";
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var path = FileDialogs.Save(((App)Application.Current).WindowHandle, "Save report", "Text file (*.txt)|*.txt", "clarion-report.txt");
        if (path is null) return;
        try
        {
            File.WriteAllText(path, $"{_title}{Environment.NewLine}{Environment.NewLine}{_body}");
            StatusLabel.Text = "Saved.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            StatusLabel.Text = ex.Message;
        }
    }

    private static void CopyToClipboard(string text)
    {
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
    }
}
