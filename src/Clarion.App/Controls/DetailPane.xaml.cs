using Clarion.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Controls;

/// <summary>The right hand side panel that explains whatever is selected, shared by every list page.</summary>
public sealed partial class DetailPane : UserControl
{
    public DetailPane()
    {
        InitializeComponent();
    }

    private TweakItem? _shown;
    private bool _loading;

    public string EmptyMessage { get => EmptyText.Text; set => EmptyText.Text = value; }
    public string TechnicalTitle { get => Detail.TechnicalTitle; set => Detail.TechnicalTitle = value; }

    /// <summary>Shows the item, or the empty message when there is none.</summary>
    public void Show(IDetailSource? item, bool technical)
    {
        _shown = item as TweakItem;
        Detail.Item = item;
        Detail.ShowTechnical = technical;
        Body.Visibility = item is null ? Visibility.Collapsed : Visibility.Visible;
        ShowDeprovision(technical);
        var note = _shown?.ImageNote ?? "";
        ImageNoteText.Text = note;
        ImageNoteText.Visibility = note.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        EmptyText.Visibility = item is null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ShowDeprovision(bool expert)
    {
        var item = _shown;
        DeprovisionPanel.Visibility = expert && item is { CanDeprovision: true } ? Visibility.Visible : Visibility.Collapsed;
        if (item is not { CanDeprovision: true }) return;

        var elevated = AppServices.Instance.IsElevated;
        _loading = true;
        DeprovisionBox.IsChecked = item.DeprovisionToo;
        DeprovisionBox.IsEnabled = elevated && !item.DeprovisionLocked;
        _loading = false;
        DeprovisionNote.Text = item.DeprovisionLocked
            ? "Done. Revert puts the app back for you only. New accounts get it from the Microsoft Store."
            : elevated
                ? "Expert option. Needs administrator rights. Revert puts the app back for you only, new accounts would need the Microsoft Store."
                : "Needs Clarion to run as administrator.";
    }

    private void OnDeprovisionChanged(object sender, RoutedEventArgs e)
    {
        if (_loading || _shown is null) return;
        AppServices.Instance.SetDeprovision(_shown, DeprovisionBox.IsChecked == true);
        ShowDeprovision(expert: true);
    }
}
