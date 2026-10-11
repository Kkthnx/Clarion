using Clarion.Core.Apps;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;

namespace Clarion.App.Controls;

/// <summary>An app's logo, drawn from its outline in the brand colour, or a plain tile with its first letter.</summary>
public sealed partial class AppMark : UserControl
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(AppIcon), typeof(AppMark), new PropertyMetadata(null, (d, _) => ((AppMark)d).Refresh()));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(AppMark), new PropertyMetadata("", (d, _) => ((AppMark)d).Refresh()));

    public static readonly DependencyProperty IsWatermarkProperty =
        DependencyProperty.Register(nameof(IsWatermark), typeof(bool), typeof(AppMark), new PropertyMetadata(false, (d, _) => ((AppMark)d).Refresh()));

    private string? _shownPath;

    public AppMark()
    {
        InitializeComponent();
        // The colour depends on the theme, so it is worked out again when the theme changes.
        Loaded += (_, _) => { ActualThemeChanged += OnThemeChanged; Refresh(); };
        Unloaded += (_, _) => ActualThemeChanged -= OnThemeChanged;
    }

    public AppIcon? Icon { get => (AppIcon?)GetValue(IconProperty); set => SetValue(IconProperty, value); }

    /// <summary>The app's name. Its first character is shown when there is no logo.</summary>
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }

    /// <summary>A faded mark behind other content. With no logo nothing is drawn, since a lone letter would only be noise.</summary>
    public bool IsWatermark { get => (bool)GetValue(IsWatermarkProperty); set => SetValue(IsWatermarkProperty, value); }

    private void OnThemeChanged(FrameworkElement sender, object args) => Refresh();

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => Letter.FontSize = Math.Max(10, Math.Min(e.NewSize.Width, e.NewSize.Height) * 0.45);

    private void Refresh()
    {
        if (Icon is { } icon)
        {
            Tile.Visibility = Visibility.Collapsed;
            Glyph.Visibility = Visibility.Visible;
            if (_shownPath != icon.Path)
            {
                Glyph.Data = (Geometry)XamlBindingHelper.ConvertValue(typeof(Geometry), icon.Path);
                _shownPath = icon.Path;
            }
            // A dark logo on a dark card, or a pale one on a light card, would disappear, so the text colour stands in.
            var dark = ActualTheme != ElementTheme.Light;
            if (icon.ReadableOn(dark))
            {
                var (r, g, b) = icon.Rgb;
                Glyph.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(255, r, g, b));
            }
            else
            {
                Glyph.Fill = ThemeBrushes.Get("ClTextPrimary", ActualTheme);
            }
            return;
        }

        Glyph.Visibility = Visibility.Collapsed;
        _shownPath = null;
        Tile.Visibility = IsWatermark ? Visibility.Collapsed : Visibility.Visible;
        Letter.Text = Label.Length > 0 ? char.ToUpperInvariant(Label[0]).ToString() : "";
    }
}
