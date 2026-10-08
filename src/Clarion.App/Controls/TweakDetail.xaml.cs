using Clarion.App.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Controls;

/// <summary>The full explanation of one setting. Used inside tooltips and in the detail pane.</summary>
public sealed partial class TweakDetail : UserControl
{
    public static readonly DependencyProperty ItemProperty =
        DependencyProperty.Register(nameof(Item), typeof(IDetailSource), typeof(TweakDetail),
            new PropertyMetadata(null, (d, _) => ((TweakDetail)d).Bindings?.Update()));

    public static readonly DependencyProperty ShowTechnicalProperty =
        DependencyProperty.Register(nameof(ShowTechnical), typeof(bool), typeof(TweakDetail),
            new PropertyMetadata(false, (d, _) => ((TweakDetail)d).Bindings?.Update()));

    public static readonly DependencyProperty TechnicalTitleProperty =
        DependencyProperty.Register(nameof(TechnicalTitle), typeof(string), typeof(TweakDetail),
            new PropertyMetadata("Exact changes", (d, _) => ((TweakDetail)d).Bindings?.Update()));

    public string TechnicalTitle { get => (string)GetValue(TechnicalTitleProperty); set => SetValue(TechnicalTitleProperty, value); }

    public TweakDetail()
    {
        InitializeComponent();
    }

    public IDetailSource? Item { get => (IDetailSource?)GetValue(ItemProperty); set => SetValue(ItemProperty, value); }
    public bool ShowTechnical { get => (bool)GetValue(ShowTechnicalProperty); set => SetValue(ShowTechnicalProperty, value); }
}
