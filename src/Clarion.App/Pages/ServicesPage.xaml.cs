using Clarion.App.Controls;
using Clarion.App.Services;
using Clarion.Core.WindowsServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

/// <summary>
/// Every Windows service on this PC beside what Microsoft's table says about it. It only reads. Changing a service is done through
/// a setting, which is recorded and can be reverted, so the row for a service that has one links to it.
/// </summary>
public sealed partial class ServicesPage : Page
{
    private readonly AppServices _app = AppServices.Instance;
    private IReadOnlyList<ServiceRow> _rows = [];
    private ServiceGuide? _guide;

    public ServicesPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e) => _ = LoadAsync();

    private async Task LoadAsync()
    {
        ReadingPanel.Visibility = Visibility.Visible;
        List.Visibility = Visibility.Collapsed;
        Empty.Visibility = Visibility.Collapsed;
        Summary.Text = "";
        try
        {
            var guide = ServiceGuide.LoadEmbedded();
            // Copied on this thread: the list belongs to the window, and the read below runs on another one.
            var tweaks = _app.Items.Select(i => i.Tweak).ToList();
            var on = _app.Items.ToDictionary(i => i.Id, i => i.IsApplied, StringComparer.OrdinalIgnoreCase);
            var lister = _app.Runtime.Services;
            _rows = await Task.Run(() => ServiceRows.Build(lister.List(), guide, tweaks, t => on.GetValueOrDefault(t.Id)));
            _guide = guide;
            SourceLink.Content = $"Read Microsoft's table, dated {guide.Dated}";
            Summary.Text = $"{_rows.Count} services. {_rows.Count(r => r.Service.Running)} running. " +
                $"{_rows.Count(r => r.DiffersFromGuide)} differ from Microsoft's table. {_rows.Count(r => r.Setting is not null)} have a .Clarion setting.";
            Show();
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or System.Text.Json.JsonException)
        {
            Log.Write($"The services could not be read: {ex.Message}");
            Summary.Text = "The services could not be read on this PC.";
            List.ItemsSource = null;
        }
        finally
        {
            ReadingPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void Show()
    {
        var filter = FilterBox.SelectedItem is ComboBoxItem { Tag: string tag } && Enum.TryParse<ServiceFilter>(tag, out var f) ? f : ServiceFilter.All;
        var rows = ServiceRows.Filter(_rows, filter, Search.Text);
        List.ItemsSource = rows.Select(r => new ServiceRowItem(r)).ToList();
        List.Visibility = rows.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        Empty.Visibility = rows.Count > 0 || _rows.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e) { if (_guide is not null) Show(); }

    private void OnFilterChanged(object sender, SelectionChangedEventArgs e) { if (_guide is not null) Show(); }

    private void OnSource(object sender, RoutedEventArgs e) { if (_guide is not null) Shell.Open(_guide.Source); }

    private void OnOpenSetting(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: string name } && name.Length > 0) _app.RequestSearch(name);
    }
}

/// <summary>One row of the list, with its words and chip colors worked out, so the template has nothing to decide.</summary>
public sealed class ServiceRowItem
{
    public ServiceRowItem(ServiceRow r)
    {
        var s = r.Service;
        Title = s.DisplayName.Length > 0 ? s.DisplayName : s.Name;
        Code = s.Name;
        StartText = s.Start is { } start ? ServiceGuide.StartLabel(start) : "Start type unknown";

        RunKind = s.Running ? ChipKind.Cyan : ChipKind.Neutral;
        RunText = !s.Running ? "Stopped" : s.MemoryBytes is { } m ? $"Running, {m / 1048576.0:0.0} MB" : s.SharedHost ? "Running, shared process" : "Running";
        RunTip = s.Running
            ? "The memory is the working set of the process that hosts this service, right now. A service that shares its process with others shows none."
            : "Not running now. A service that is set to Manual starts when something needs it.";

        if (r.Guide is { } g)
        {
            AdviceText = ServiceGuide.Label(g.Advice);
            AdviceTip = ServiceGuide.Hint(g.Advice);
            AdviceKind = g.Advice switch
            {
                ServiceAdvice.OkToDisable => ChipKind.Ok,
                ServiceAdvice.DontDisable => ChipKind.Warn,
                ServiceAdvice.ShouldDisable => ChipKind.Accent,
                _ => ChipKind.Neutral,
            };
            if (r.DiffersFromGuide)
            {
                GuideVisible = Visibility.Visible;
                GuideText = $"Table says {ServiceGuide.StartLabel(g.Default)}";
            }
        }
        else
        {
            AdviceText = "Not in Microsoft's table";
            AdviceTip = "Microsoft's table does not list this service, so there is no recommendation. It may belong to a driver, a program or a part of Windows the table leaves out.";
        }

        DemandVisible = s.StartsOnDemand ? Visibility.Visible : Visibility.Collapsed;
        ProtectedVisible = r.Protected ? Visibility.Visible : Visibility.Collapsed;
        if (r.Setting is { } t)
        {
            SettingVisible = Visibility.Visible;
            SettingName = t.Name;
            SettingText = $"Setting: {t.Name}. It is {(r.SettingOn ? "on" : "off")}.";
            SettingButtonName = $"Open the setting {t.Name}";
        }
    }

    public string Title { get; }
    public string Code { get; }
    public string StartText { get; }
    public string RunText { get; }
    public ChipKind RunKind { get; }
    public string RunTip { get; }
    public string AdviceText { get; }
    public ChipKind AdviceKind { get; }
    public string AdviceTip { get; }
    public string GuideText { get; } = "";
    public Visibility GuideVisible { get; } = Visibility.Collapsed;
    public string GuideTip { get; } =
        "Microsoft's table is for Windows IoT Enterprise and gives this as the default. Defaults on Home and Pro can differ, so this is not proof that anyone changed it.";
    public Visibility DemandVisible { get; }
    public Visibility ProtectedVisible { get; }
    public Visibility SettingVisible { get; } = Visibility.Collapsed;
    public string SettingName { get; } = "";
    public string SettingText { get; } = "";
    public string SettingButtonName { get; } = "";
}
