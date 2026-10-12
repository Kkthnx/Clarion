using Clarion.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Controls;

/// <summary>
/// The circled i beside a setting, a repair job or a cleanup row. It shows what a pane on the right used to: what the item
/// does, what you gain, what could go wrong and, in Expert mode, the exact changes and sources. A flyout, not a tooltip, because
/// it holds links and a tick box, and because a tooltip cannot be reached from the keyboard or by touch.
/// </summary>
public sealed partial class InfoButton : UserControl
{
    public static readonly DependencyProperty ItemProperty =
        DependencyProperty.Register(nameof(Item), typeof(IDetailSource), typeof(InfoButton), new PropertyMetadata(null, (d, _) => ((InfoButton)d).OnItemChanged()));

    public static readonly DependencyProperty AlwaysTechnicalProperty =
        DependencyProperty.Register(nameof(AlwaysTechnical), typeof(bool), typeof(InfoButton), new PropertyMetadata(false));

    public static readonly DependencyProperty TechnicalTitleProperty =
        DependencyProperty.Register(nameof(TechnicalTitle), typeof(string), typeof(InfoButton), new PropertyMetadata("Exact changes", (d, e) => { if (((InfoButton)d).Pane is { } pane) pane.TechnicalTitle = (string)e.NewValue; }));

    public InfoButton()
    {
        InitializeComponent();
    }

    public IDetailSource? Item { get => (IDetailSource?)GetValue(ItemProperty); set => SetValue(ItemProperty, value); }

    /// <summary>True for pages whose exact commands or folders are always shown. Otherwise they appear in Expert mode only.</summary>
    public bool AlwaysTechnical { get => (bool)GetValue(AlwaysTechnicalProperty); set => SetValue(AlwaysTechnicalProperty, value); }

    public string TechnicalTitle { get => (string)GetValue(TechnicalTitleProperty); set => SetValue(TechnicalTitleProperty, value); }

    private void OnItemChanged()
    {
        // A screen reader says the button's name, so it names what the explanation is about.
        AutomationProperties.SetName(Trigger, Item is null ? "Details" : $"Details about {Item.Name}");
    }

    private void OnOpening(object? sender, object e) => Pane.Show(Item, AlwaysTechnical || AppServices.Instance.ExpertMode);
}
