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

    public string EmptyMessage { get => EmptyText.Text; set => EmptyText.Text = value; }
    public string TechnicalTitle { get => Detail.TechnicalTitle; set => Detail.TechnicalTitle = value; }

    /// <summary>Shows the item, or the empty message when there is none.</summary>
    public void Show(IDetailSource? item, bool technical)
    {
        Detail.Item = item;
        Detail.ShowTechnical = technical;
        Detail.Visibility = item is null ? Visibility.Collapsed : Visibility.Visible;
        EmptyText.Visibility = item is null ? Visibility.Visible : Visibility.Collapsed;
    }
}
