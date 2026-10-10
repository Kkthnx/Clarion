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
        // A greyed switch with no reason reads as broken. Say what is going on, and that it comes back on its own.
        if (note.Length == 0 && _shown is { IsSupported: true, IsUnavailable: true } gone)
            note = gone.RemovesApps
                ? "This app is not installed for your account, so there is nothing to remove. If Windows brings it back, for example after a feature update, this switch works again."
                : "What this changes is not on this PC, so there is nothing to change. If Windows puts it back, this switch works again.";
        ImageNoteText.Text = note;
        ImageNoteText.Visibility = note.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        EmptyPanel.Visibility = item is null ? Visibility.Visible : Visibility.Collapsed;
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
        // A customized image often has no Store at all. Then the way back is not "get it from the Store" but "restore the Store first".
        var storeMissing = AppServices.Instance.ImageVerdict?.Inputs?.StoreMissing == true;
        DeprovisionNote.Text = storeMissing
            ? (item.DeprovisionLocked ? "Done. " : "Think before you tick this. ") +
              "The Microsoft Store is not installed on this PC. Revert puts the app back for you only, and only if Windows still has its files. " +
              "For new accounts there is no way back until the Store itself is restored, and Clarion cannot do that for you."
            : item.DeprovisionLocked
                ? "Done. Revert puts the app back for you only. New accounts get it from the Microsoft Store."
                : elevated
                    ? "Expert option. Needs administrator rights. Revert puts the app back for you only, new accounts would need the Microsoft Store."
                    : "Needs Clarion to run as administrator.";
        DeprovisionNote.Foreground = storeMissing
            ? ThemeBrushes.Get("ClWarn", ActualTheme)
            : ThemeBrushes.Get("ClTextSecondary", ActualTheme);
    }

    private void OnDeprovisionChanged(object sender, RoutedEventArgs e)
    {
        if (_loading || _shown is null) return;
        AppServices.Instance.SetDeprovision(_shown, DeprovisionBox.IsChecked == true);
        ShowDeprovision(expert: true);
    }
}
