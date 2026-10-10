using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Controls;

/// <summary>A small card for one fact about the PC: a colored icon, a title, the main value and a line of detail.</summary>
public sealed partial class InfoTile : UserControl
{
    public static readonly DependencyProperty GlyphProperty = Register(nameof(Glyph), "");
    public static readonly DependencyProperty TitleProperty = Register(nameof(Title), "");
    public static readonly DependencyProperty ValueProperty = Register(nameof(Value), "");
    public static readonly DependencyProperty DetailProperty = Register(nameof(Detail), "");

    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(nameof(Kind), typeof(ChipKind), typeof(InfoTile), new PropertyMetadata(ChipKind.Accent, (d, _) => ((InfoTile)d).Refresh()));

    public InfoTile()
    {
        InitializeComponent();
        Loaded += (_, _) => Refresh();
    }

    public string Glyph { get => (string)GetValue(GlyphProperty); set => SetValue(GlyphProperty, value); }
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public string Detail { get => (string)GetValue(DetailProperty); set => SetValue(DetailProperty, value); }
    public ChipKind Kind { get => (ChipKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }

    private static DependencyProperty Register(string name, string initial) =>
        DependencyProperty.Register(name, typeof(string), typeof(InfoTile), new PropertyMetadata(initial, (d, _) => ((InfoTile)d).Refresh()));

    private void Refresh()
    {
        Icon.Glyph = Glyph;
        TitleText.Text = Title;
        ValueText.Text = Value;
        DetailText.Text = Detail;
        DetailText.Visibility = Detail.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
        // Only the colors a tile uses have a state. Anything else keeps the neutral icon box.
        VisualStateManager.GoToState(this, Kind is ChipKind.Accent or ChipKind.Cyan or ChipKind.Ok or ChipKind.Warn ? Kind.ToString() : "Accent", false);
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(this, $"{Title}: {Value}. {Detail}".TrimEnd(' ', '.'));
        ToolTipService.SetToolTip(this, Value.Length + Detail.Length == 0 ? null : (Detail.Length > 0 ? $"{Value}\n{Detail}" : Value));
    }
}
