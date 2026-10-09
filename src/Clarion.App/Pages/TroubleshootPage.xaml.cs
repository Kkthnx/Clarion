using Clarion.App.Services;
using Clarion.Core.Catalog;
using Clarion.Core.Troubleshoot;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Clarion.App.Pages;

public sealed partial class TroubleshootPage : Page
{
    private readonly AppServices _app = AppServices.Instance;
    private readonly IReadOnlyList<Symptom> _symptoms = CatalogLoader.LoadSymptoms();

    public TroubleshootPage()
    {
        InitializeComponent();
        SymptomBox.ItemsSource = _symptoms.Select(s => s.Title).ToList();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) => Show();

    private void OnSymptomChanged(object sender, SelectionChangedEventArgs e) => Show();

    private void Show()
    {
        var index = SymptomBox.SelectedIndex;
        HintText.Visibility = index < 0 ? Visibility.Visible : Visibility.Collapsed;
        if (index < 0)
        {
            TipText.Visibility = Visibility.Collapsed;
            NoneText.Visibility = Visibility.Collapsed;
            Rows.ItemsSource = null;
            return;
        }

        var symptom = _symptoms[index];
        TipText.Text = symptom.Tip;
        TipText.Visibility = Visibility.Visible;

        var rows = _app.SuspectsFor(symptom).Select(s => Row(s)).ToList();
        Rows.ItemsSource = rows;
        NoneText.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private SuspectRow Row(Suspect s)
    {
        var item = _app.Items.FirstOrDefault(i => i.Id == s.Tweak.Id);
        var canRevert = item is { CanToggle: true, IsApplied: true };
        var meta = $"Applied {s.AppliedAt.LocalDateTime:g}. " + (s.Direct ? "Directly related to this." : "Same area as this.");
        return new SuspectRow(s.Tweak.Id, s.Tweak.Name, s.Tweak.Summary, meta, canRevert,
            canRevert ? "Put back what was there before" : "Windows has already changed this back, or it cannot be reverted right now");
    }

    private void OnRevert(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string id }) _app.QueueRevert(id);
    }
}

public sealed record SuspectRow(string TweakId, string Name, string Summary, string Meta, bool CanRevert, string RevertHint);
