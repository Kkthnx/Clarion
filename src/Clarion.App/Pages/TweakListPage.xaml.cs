using System.Collections.ObjectModel;
using Clarion.App.Services;
using Clarion.Core.Model;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed record PageArgs(string Title, string Subtitle, string[] Categories);

public sealed partial class TweakListPage : Page
{
    private readonly AppServices _app = AppServices.Instance;
    private PageArgs _args = new("", "", []);
    private string _query = "";

    public TweakListPage()
    {
        InitializeComponent();
        Visible = [];
        _app.ExpertModeChangedByUser += OnModeChanged;
        Unloaded += (_, _) => _app.ExpertModeChangedByUser -= OnModeChanged;
    }

    public ObservableCollection<TweakItem> Visible { get; }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is PageArgs args) _args = args;
        TitleText.Text = _args.Title;
        SubtitleText.Text = _args.Subtitle;
        Rebuild();
    }

    private void OnModeChanged(object? sender, EventArgs e) => DispatcherQueue.TryEnqueue(Rebuild);

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        _query = sender.Text.Trim();
        Rebuild();
    }

    private void Rebuild()
    {
        var selected = (List.SelectedItem as TweakItem)?.Id;
        var items = _app.Items
            .Where(i => _args.Categories.Contains(i.Category, StringComparer.OrdinalIgnoreCase))
            .Where(i => _app.ExpertMode || (i.Tweak.RiskLevel <= RiskLevel.Low && i.Tweak.Evidence != Evidence.Unproven))
            .Where(Matches)
            .OrderBy(i => i.IsSupported ? (i.IsUnavailable ? 1 : 0) : 2)
            .ThenBy(i => i.Tweak.Recommendation)
            .ThenBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        Visible.Clear();
        foreach (var i in items) Visible.Add(i);
        if (selected is not null) List.SelectedItem = items.FirstOrDefault(i => i.Id == selected);
        UpdateDetail();
    }

    private bool Matches(TweakItem i) =>
        _query.Length == 0 ||
        i.Name.Contains(_query, StringComparison.CurrentCultureIgnoreCase) ||
        i.Summary.Contains(_query, StringComparison.CurrentCultureIgnoreCase) ||
        i.ExactChanges.Any(c => c.Contains(_query, StringComparison.CurrentCultureIgnoreCase));

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateDetail();

    private void UpdateDetail()
    {
        if (List.SelectedItem is TweakItem item)
        {
            Detail.Item = item;
            Detail.ShowTechnical = _app.ExpertMode;
            Detail.Visibility = Visibility.Visible;
            EmptyDetail.Visibility = Visibility.Collapsed;
        }
        else
        {
            Detail.Visibility = Visibility.Collapsed;
            EmptyDetail.Visibility = Visibility.Visible;
        }
    }
}
