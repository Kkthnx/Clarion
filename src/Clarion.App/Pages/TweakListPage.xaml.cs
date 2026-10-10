using System.Collections.ObjectModel;
using Clarion.App.Controls;
using Clarion.App.Services;
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
        };
    }

    public ObservableCollection<TweakItem> Visible { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is PageArgs args) _args = args;
        TitleText.Text = _args.Title;
        SubtitleText.Text = _args.Subtitle;
        _query = _args.Query;
        SearchBox.Text = _query;
        _compact = _app.Settings.CompactLists;
        ApplyDensity();
        Rebuild();
    }

    /// <summary>The compact list is one line per setting. The choice is remembered, and the page keeps its selection when it changes.</summary>
    private void ApplyDensity()
    {
        CompactToggle.IsChecked = _compact;
        List.ItemTemplate = (DataTemplate)Resources[_compact ? "CompactRow" : "RoomyRow"];
        List.ItemContainerStyle = (Style)Application.Current.Resources[_compact ? "CompactListViewItemStyle" : "CardListViewItemStyle"];
        // One line per setting reads best with room for the name, so the list takes more of the width and the pane less.
        ListColumn.Width = new GridLength(_compact ? 1.5 : 1, GridUnitType.Star);
        DetailColumn.Width = new GridLength(_compact ? 1 : 1.05, GridUnitType.Star);
    }

    private void OnCompactClicked(object sender, RoutedEventArgs e)
    {
        _compact = CompactToggle.IsChecked == true;
        _app.UpdateSettings(s => s with { CompactLists = _compact });
        var selected = (List.SelectedItem as TweakItem)?.Id;
        ApplyDensity();
        if (selected is not null) List.SelectedItem = Visible.FirstOrDefault(i => i.Id == selected);
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
            var selected = (List.SelectedItem as TweakItem)?.Id;
            var inScope = _app.Items
                .Where(i => _args.Categories.Length == 0 || _args.Categories.Contains(i.Category, StringComparer.OrdinalIgnoreCase))
                .Where(i => _app.ExpertMode || (i.Tweak.RiskLevel <= RiskLevel.Low && i.Tweak.Evidence != Evidence.Unproven))
                .ToList();

            FillTopics(inScope);
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

            Visible.Clear();
            foreach (var i in items) Visible.Add(i);
            var total = useChooser ? inScope.Count - dns.Count + 1 : inScope.Count;
            var shown = items.Count + (Chooser.Visibility == Visibility.Visible ? 1 : 0);
            CountText.Text = shown == total ? $"{shown} settings" : $"{shown} of {total} settings";
            if (selected is not null) List.SelectedItem = items.FirstOrDefault(i => i.Id == selected);
            UpdateDetail();
        }
        finally
        {
            _building = false;
        }
    }

    private bool Matches(TweakItem i) =>
        _query.Length == 0 ||
        i.Name.Contains(_query, StringComparison.CurrentCultureIgnoreCase) ||
        i.Summary.Contains(_query, StringComparison.CurrentCultureIgnoreCase) ||
        i.ExactChanges.Any(c => c.Contains(_query, StringComparison.CurrentCultureIgnoreCase));

    private bool PassesFilter(TweakItem i) => _filter switch
    {
        "suggested" => i.Tweak.Recommendation == Recommendation.Recommended && i.CanToggle,
        "on" => i.IsApplied && !i.IsUnavailable,
        "off" => i.CanToggle && !i.IsApplied,
        "available" => i.CanToggle,
        "attention" => i.IsDrifted || i.HasLastError || (i.Tweak.Recommendation == Recommendation.Recommended && i.CanToggle && !i.IsApplied),
        _ => true,
    };

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateDetail();

    private void OnTipOpened(object sender, RoutedEventArgs e) => DetailHelpers.FillTipOnOpen(sender);

    private void UpdateDetail() => Pane.Show(List.SelectedItem as TweakItem, _app.ExpertMode);
}
