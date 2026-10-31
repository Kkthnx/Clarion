using System.Collections.ObjectModel;
using Clarion.App.Controls;
using Clarion.App.Services;
using Clarion.Core.Layout;
using Clarion.Core.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed record PageArgs(string Title, string Subtitle, string[] Categories, string Query = "");

public sealed partial class TweakListPage : Page
{
    private readonly AppServices _app = AppServices.Instance;
    private PageArgs _args = new("", "", []);
    private string _query = "";
    private string _filter = "all";
    private string _topic = "";
    private string _pillsBuiltFor = "";
    private bool _compact;
    private bool _building;
    private readonly bool _ready;

    public TweakListPage()
    {
        InitializeComponent();
        Visible = [];
        _ready = true;
        _app.ExpertModeChangedByUser += OnModeChanged;
        _app.StatesRefreshed += OnModeChanged;
        Unloaded += (_, _) =>
        {
            _app.ExpertModeChangedByUser -= OnModeChanged;
            _app.StatesRefreshed -= OnModeChanged;
            // The rows are the program's own settings, so the bindings in them must be told to stop listening. See PageRelease.
            PageRelease.Lists(this);
        };
    }

    public ObservableCollection<TweakItem> Visible { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is PageArgs args) _args = args;
        TitleText.Text = _args.Title;
        // Inside the Windows page the selected tab already names the list, so the title is left out and the line under it stays.
        TitleText.Visibility = _args.Title.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        SubtitleText.Text = _args.Subtitle;
        _query = _args.Query;
        SearchBox.Text = _query;
        _compact = _app.Settings.CompactLists;
        ApplyDensity();
        Rebuild();
    }

    /// <summary>The compact list is one line per setting. The choice is remembered.</summary>
    private void ApplyDensity()
    {
        CompactToggle.IsChecked = _compact;
        // The columns decide the template, since two cards in a row use another one. A change of density asks again.
        _columns = 0;
        ApplyColumns();
    }

    // More than one card across when the window is wide enough, and never in the compact list. Each card needs about CardColumns.MinCardWidth for its
    // name, summary and switch, so a narrower list keeps one wide card, which reads better.
    private int _columns;

    private void OnListSizeChanged(object sender, SizeChangedEventArgs e) => ApplyColumns();

    private void ApplyColumns()
    {
        var wanted = CardColumns.For(List.ActualWidth, _compact);
        if (wanted == _columns && List.ItemsSource is not null) return;
        _columns = wanted;
        if (wanted > 1)
        {
            List.ItemTemplate = (DataTemplate)Resources[wanted == 2 ? "TwoColumnRow" : "ThreeColumnRow"];
            List.ItemContainerStyle = (Style)Resources["PairRowItemStyle"];
            FillRows();
        }
        else
        {
            List.ItemTemplate = (DataTemplate)Resources[_compact ? "CompactRow" : "RoomyRow"];
            List.ItemContainerStyle = (Style)Application.Current.Resources[_compact ? "CompactListViewItemStyle" : "CardListViewItemStyle"];
            List.ItemsSource = Visible;
        }
    }

    // The rows are cut from the list in one go and handed over as one list, so the page changes once and not once per row.
    private void FillRows() => List.ItemsSource = CardRow.Of(Visible, _columns);
    // A card in a row of several lights up under the pointer like a card in the single column does.
    private void OnCardEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Border card) return;
        card.Background = ThemeBrushes.Get("ClSurface2", ActualTheme);
        card.BorderBrush = ThemeBrushes.Get("ClAccent", ActualTheme);
    }

    private void OnCardExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is not Border card) return;
        card.Background = ThemeBrushes.Get("ClSurface1", ActualTheme);
        card.BorderBrush = ThemeBrushes.Get("ClBorder", ActualTheme);
    }
    private void OnCompactClicked(object sender, RoutedEventArgs e)
    {
        _compact = CompactToggle.IsChecked == true;
        _app.UpdateSettings(s => s with { CompactLists = _compact });
        ApplyDensity();
    }

    private void OnModeChanged(object? sender, EventArgs e) => DispatcherQueue.TryEnqueue(Rebuild);

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (!_ready) return;
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
        _query = sender.Text.Trim();
        Rebuild();
    }

    private void OnPillClicked(object sender, RoutedEventArgs e)
    {
        if (!_ready || sender is not ToggleButton { Tag: string topic }) return;
        _topic = topic;
        Rebuild();
    }

    /// <summary>
    /// One pill per topic with how many settings it holds, and one for all of them. The pills are made again only when the topics or their
    /// counts change, so pressing one does not rebuild the row it was pressed in.
    /// </summary>
    private void FillTopics(IEnumerable<TweakItem> scope)
    {
        var list = scope.ToList();
        // The DNS providers show as one row (the chooser), so they count as one here, not as one each.
        var dnsCount = list.Count(DnsChooser.IsDnsItem);
        var rows = list.Count - dnsCount + (dnsCount > 0 ? 1 : 0);
        var counts = list.Where(i => i.Tweak.Topic.Length > 0).GroupBy(i => i.Tweak.Topic).OrderBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase)
            .Select(g => (Topic: g.Key, Count: g.Count(i => !DnsChooser.IsDnsItem(i)) + (g.Any(DnsChooser.IsDnsItem) ? 1 : 0))).ToList();
        if (!counts.Any(c => c.Topic == _topic)) _topic = "";

        // Narrowing a short list takes more room than it saves, so the pills wait for a list worth narrowing.
        var worthIt = counts.Count > 1 && rows > 6;
        if (!worthIt) _topic = "";
        var key = string.Join("|", counts.Select(c => $"{c.Topic}:{c.Count}")) + $"|{rows}";
        if (key != _pillsBuiltFor)
        {
            _pillsBuiltFor = key;
            TopicPills.Children.Clear();
            if (worthIt)
            {
                TopicPills.Children.Add(Pill("", "All", rows));
                foreach (var (topic, count) in counts) TopicPills.Children.Add(Pill(topic, topic, count));
            }
        }
        TopicPills.Visibility = worthIt ? Visibility.Visible : Visibility.Collapsed;
        foreach (var pill in TopicPills.Children.OfType<ToggleButton>()) pill.IsChecked = (pill.Tag as string) == _topic;
    }

    private ToggleButton Pill(string topic, string label, int count)
    {
        var pill = new ToggleButton
        {
            Content = $"{label} · {count}",
            Tag = topic,
            Padding = new Thickness(12, 4, 12, 4),
            MinHeight = 30,
            CornerRadius = new CornerRadius(15),
        };
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(pill, $"{label}, {count} settings");
        pill.Click += OnPillClicked;
        return pill;
    }

    private void OnFilterChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready) return;
        _filter = (FilterBox.SelectedItem as ComboBoxItem)?.Tag as string ?? "all";
        Rebuild();
    }

    private void Rebuild()
    {
        if (_building || !_ready) return;
        _building = true;
        try
        {
            var inScope = _app.Items
                .Where(i => _args.Categories.Length == 0 || _args.Categories.Contains(i.Category, StringComparer.OrdinalIgnoreCase))
                .Where(i => _app.ExpertMode || (i.Tweak.RiskLevel <= RiskLevel.Low && i.Tweak.Evidence != Evidence.Unproven))
                .ToList();

            // The pills count what the search and the filter leave, so a pill never promises more settings than pressing it shows.
            FillTopics(inScope.Where(Matches).Where(PassesFilter));
            var dns = inScope.Where(DnsChooser.IsDnsItem).ToList();
            var useChooser = dns.Count > 0 && _query.Length == 0 && _filter == "all";
            Chooser.Visibility = useChooser && (_topic.Length == 0 || _topic == dns[0].Tweak.Topic) ? Visibility.Visible : Visibility.Collapsed;
            if (useChooser) Chooser.Bind(dns);
            var items = inScope
                .Where(i => !useChooser || !DnsChooser.IsDnsItem(i))
                .Where(i => _topic.Length == 0 || i.Tweak.Topic == _topic)
                .Where(Matches)
                .Where(PassesFilter)
                .OrderBy(i => i.IsSupported ? (i.IsUnavailable ? 1 : 0) : 2)
                .ThenBy(i => i.Tweak.Recommendation)
                .ThenBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            // A list that comes out the same as the one on screen is left alone, so pressing Re-check or a read finishing does not throw the
            // person back to the top of a long list. Each row follows its own setting, so a changed switch needs no new list.
            var same = Visible.Count == items.Count && Visible.Zip(items).All(p => ReferenceEquals(p.First, p.Second));
            if (!same)
            {
                Visible.Clear();
                foreach (var i in items) Visible.Add(i);
            }
            // Which apps are installed is read the first time a list shows one, not only on the Apps page. A search or a link can show an app
            // anywhere, and its switch stays on Checking until the read has been asked for.
            if (items.Any(i => i.InstallsApps)) _ = _app.EnsureAppsReadAsync();
            if (_columns > 1 && !same) FillRows();
            var total = useChooser ? inScope.Count - dns.Count + 1 : inScope.Count;
            var shown = items.Count + (Chooser.Visibility == Visibility.Visible ? 1 : 0);
            CountText.Text = shown == total ? $"{shown} settings" : $"{shown} of {total} settings";
            var hidesRiskier = !_app.ExpertMode;
            EmptyText.Text = shown == 0 ? SearchMatch.EmptyMessage(_query, _filter != "all" || _topic.Length > 0, hidesRiskier) : "";
            EmptyText.Visibility = shown == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        finally
        {
            _building = false;
        }
    }

    private bool Matches(TweakItem i) =>
        _query.Length == 0 || SearchMatch.Matches(_query, [i.Name, i.Summary, i.Tweak.Topic, i.Category, .. i.ExactChanges]);

    private bool PassesFilter(TweakItem i) => _filter switch
    {
        "suggested" => i.Tweak.Recommendation == Recommendation.Recommended && i.CanToggle,
        "on" => i.IsApplied && !i.IsUnavailable,
        "off" => i.CanToggle && !i.IsApplied,
        "available" => i.CanToggle,
        "attention" => i.IsDrifted || i.HasLastError || (i.Tweak.Recommendation == Recommendation.Recommended && i.CanToggle && !i.IsApplied),
        _ => true,
    };

}
