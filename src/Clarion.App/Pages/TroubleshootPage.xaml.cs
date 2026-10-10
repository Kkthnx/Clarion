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
        ClearDate.Visibility = WorkedUntil.Date is null ? Visibility.Collapsed : Visibility.Visible;
        if (index < 0)
        {
            TipText.Visibility = Visibility.Collapsed;
            NonePanel.Visibility = Visibility.Collapsed;
            Rows.ItemsSource = null;
            return;
        }

        var symptom = _symptoms[index];
        TipText.Text = symptom.Tip;
        TipText.Visibility = Visibility.Visible;

        // Something that worked until a date cannot have been broken by a change made before that date.
        var since = WorkedUntil.Date;
        var all = _app.SuspectsFor(symptom);
        var shown = since is null ? all : _app.SuspectsFor(symptom, since);
        var rows = shown.Select(s => Row(s)).ToList();
        Rows.ItemsSource = rows;

        NonePanel.Visibility = rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        if (rows.Count == 0) ExplainNothingFound(all.Count - shown.Count, since);
    }

    /// <summary>
    /// "It was not me" is where most people end up, so it names the usual other causes, and says so when it is a date that hid the
    /// changes. A customized Windows image is called out by name when this PC has one.
    /// </summary>
    private void ExplainNothingFound(int hiddenByDate, DateTimeOffset? since)
    {
        NoneTitle.Text = hiddenByDate > 0
            ? $"Nothing Clarion changed since {since!.Value:MMM d, yyyy} explains this"
            : "Clarion did not change anything that explains this";

        var lines = new List<string>();
        if (hiddenByDate > 0)
            lines.Add($"{hiddenByDate} older change{(hiddenByDate == 1 ? " was" : "s were")} left out because of the date. Try an earlier date, or clear it, if you are not sure when it last worked.");
        lines.Add("What else usually causes this:");

        var image = _app.ImageVerdict;
        // The wording follows how sure the install check is. "May be" is not "looks like".
        var imageName = image is { ImageName: not null } ? $" ({image.ImageLabel})" : "";
        lines.Add(image?.Level switch
        {
            Clarion.Core.SystemInfo.InstallLevel.Likely =>
                $"• This PC looks like a customized Windows image{imageName}. Those images remove or change parts of Windows before you ever install, and that can cause this on its own.",
            Clarion.Core.SystemInfo.InstallLevel.Possible =>
                $"• This PC may be running a customized Windows image{imageName}. Those images remove or change parts of Windows before you ever install, and that can cause this on its own.",
            _ => "• A customized Windows image, if you installed one. They remove or change parts of Windows before you install.",
        });
        lines.Add("• Another cleanup or privacy tool that ran before or after Clarion.");
        lines.Add("• A driver or Windows update. Windows Update history shows what was installed and when.");
        NoneText.Text = string.Join("\n", lines);
    }

    private void OnDateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) => Show();

    private void OnClearDate(object sender, RoutedEventArgs e) => WorkedUntil.Date = null;

    private void OnOpenVerify(object sender, RoutedEventArgs e) => _app.RequestNavigate("verify");

    private void OnOpenSystem(object sender, RoutedEventArgs e) => _app.RequestNavigate("system");

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
