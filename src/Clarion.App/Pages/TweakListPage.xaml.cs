using System.Collections.ObjectModel;
using Clarion.App.Controls;
using Clarion.App.Services;
using Clarion.Core.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
    private bool _fillingTopics;
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
        Rebuild();
    }

    private void OnModeChanged(object? sender, EventArgs e) => DispatcherQueue.TryEnqueue(Rebuild);

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (!_ready) return;
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
        _query = sender.Text.Trim();
        Rebuild();
    }

    private void OnTopicChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_ready || _fillingTopics) return;
        _topic = TopicBox.SelectedIndex <= 0 ? "" : TopicBox.SelectedItem as string ?? "";
        Rebuild();
    }

    private void FillTopics(IEnumerable<TweakItem> scope)
    {
        var topics = scope.Select(i => i.Tweak.Topic).Where(t => t.Length > 0).Distinct().Order().ToList();
        _fillingTopics = true;
        TopicBox.Items.Clear();
        TopicBox.Items.Add("All topics");
        foreach (var t in topics) TopicBox.Items.Add(t);
        var index = topics.IndexOf(_topic);
        TopicBox.SelectedIndex = index >= 0 ? index + 1 : 0;
        if (index < 0) _topic = "";
        TopicBox.Visibility = topics.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        _fillingTopics = false;
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
            var items = inScope
                .Where(i => _topic.Length == 0 || i.Tweak.Topic == _topic)
                .Where(Matches)
                .Where(PassesFilter)
                .OrderBy(i => i.IsSupported ? (i.IsUnavailable ? 1 : 0) : 2)
                .ThenBy(i => i.Tweak.Recommendation)
                .ThenBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            Visible.Clear();
            foreach (var i in items) Visible.Add(i);
            CountText.Text = items.Count == inScope.Count ? $"{items.Count} settings" : $"{items.Count} of {inScope.Count} settings";
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
        "attention" => i.IsDrifted || i.HasLastError || (i.Tweak.Recommendation == Recommendation.Recommended && i.CanToggle && !i.IsApplied),
        _ => true,
    };

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateDetail();

    private void OnTipOpened(object sender, RoutedEventArgs e) => DetailHelpers.FillTipOnOpen(sender);

    private void UpdateDetail() => Pane.Show(List.SelectedItem as TweakItem, _app.ExpertMode);
}
