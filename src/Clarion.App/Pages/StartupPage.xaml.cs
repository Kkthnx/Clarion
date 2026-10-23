using Clarion.App.Services;
using Clarion.Core.Layout;
using Clarion.Core.Startup;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

/// <summary>
/// The programs that start when someone signs in, each with a switch. A change is made at once through the engine, so it is recorded
/// and can be reverted, and nothing is deleted: only the mark that Task Manager's Startup tab reads is changed.
/// </summary>
public sealed partial class StartupPage : Page
{
    private readonly AppServices _app = AppServices.Instance;
    private IReadOnlyList<StartupEntry> _entries = [];
    private bool _loaded;

    public StartupPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e) => _ = LoadAsync();

    private async Task LoadAsync()
    {
        ReadingPanel.Visibility = Visibility.Visible;
        Empty.Visibility = Visibility.Collapsed;
        try
        {
            _entries = await _app.ReadStartupAsync();
            _loaded = true;
            Show();
        }
        catch (Exception ex) when (ex is InvalidOperationException or IOException or UnauthorizedAccessException)
        {
            Log.Write($"Startup could not be read: {ex.Message}");
            Summary.Text = "What starts with Windows could not be read on this PC.";
            List.ItemsSource = null;
        }
        finally
        {
            ReadingPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void Show()
    {
        var filter = FilterBox.SelectedItem is ComboBoxItem { Tag: string tag } ? tag : "All";
        var query = Search.Text.Trim();
        var shown = _entries
            .Where(e => filter == "All" || (filter == "On") == e.Enabled)
            .Where(e => SearchMatch.Matches(query, [e.Name, e.Program.Publisher, e.Program.Description, e.Command]))
            .ToList();
        List.ItemsSource = shown.Select(e => new StartupRowItem(e, _app.IsElevated)).ToList();
        Empty.Visibility = shown.Count == 0 && _entries.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        var on = _entries.Count(e => e.Enabled);
        Summary.Text = _entries.Count == 0
            ? "Nothing is set to start with Windows from the Run keys or the Startup folders."
            : $"{on} start with Windows. {_entries.Count - on} are turned off.";
        if (!_app.IsElevated && _entries.Count > 0)
        {
            Notice.Title = ".Clarion is running without administrator rights";
            Notice.Message = "You can look at the list. Changing it needs administrator rights, because every change is recorded. Use Restart as administrator on Home.";
            Notice.IsOpen = true;
            Notice.Visibility = Visibility.Visible;
        }
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e) { if (_loaded) Show(); }

    private void OnFilterChanged(object sender, SelectionChangedEventArgs e) { if (_loaded) Show(); }

    private async void OnToggled(object sender, RoutedEventArgs e)
    {
        // The switch also fires when the row is first drawn and when a failed change is put back, and neither is a choice.
        if (sender is not ToggleSwitch { Tag: StartupRowItem row } toggle || toggle.IsOn == row.IsOn) return;
        toggle.IsEnabled = false;
        var problem = await _app.SetStartupAsync(row.Entry, toggle.IsOn);
        if (problem is null)
        {
            Notice.IsOpen = false;
            Notice.Visibility = Visibility.Collapsed;
            await LoadAsync();
            return;
        }
        Notice.Title = "That did not change";
        Notice.Message = problem;
        Notice.IsOpen = true;
        Notice.Visibility = Visibility.Visible;
        // Put the switch back where Windows still has it, without asking again.
        toggle.Toggled -= OnToggled;
        toggle.IsOn = row.IsOn;
        toggle.Toggled += OnToggled;
        toggle.IsEnabled = row.CanToggle;
    }
}

/// <summary>One row of the list with its words worked out, so the template has nothing to decide.</summary>
public sealed class StartupRowItem
{
    public StartupRowItem(StartupEntry entry, bool elevated)
    {
        Entry = entry;
        Title = entry.Name;
        Subtitle = string.Join(" · ", new[] { entry.Program.Publisher, entry.Program.Description }.Where(s => s.Length > 0));
        SubtitleVisible = Subtitle.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        Command = entry.Command;
        Source = entry.Source;
        SourceTip = entry.NeedsAdmin
            ? "Starts for every account on this PC. Changing it needs administrator rights."
            : "Starts only when you sign in.";
        IsOn = entry.Enabled;
        OffText = entry.DisabledAt is { } at ? $"Turned off {at.LocalDateTime:MMM d, yyyy}" : "Turned off";
        OffVisible = entry.Enabled ? Visibility.Collapsed : Visibility.Visible;
        Warning = entry.Warning ?? "";
        WarningVisible = Warning.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        CanToggle = elevated && !entry.IsProtected;
        ToggleName = $"Start {entry.Name} with Windows";
    }

    public StartupEntry Entry { get; }
    public string Title { get; }
    public string Subtitle { get; }
    public Visibility SubtitleVisible { get; }
    public string Command { get; }
    public string Source { get; }
    public string SourceTip { get; }
    public bool IsOn { get; }
    public string OffText { get; }
    public Visibility OffVisible { get; }
    public string Warning { get; }
    public Visibility WarningVisible { get; }
    public bool CanToggle { get; }
    public string ToggleName { get; }
}
