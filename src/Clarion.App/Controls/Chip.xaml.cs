using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Clarion.App.Controls;

public enum ChipKind { Neutral, Accent, Cyan, Ok, Warn, Danger }

/// <summary>A small colored label. Color means one thing: green safe, amber caution, red high risk, cyan live state, blue action.</summary>
public sealed partial class Chip : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(Chip), new PropertyMetadata("", (d, _) => ((Chip)d).Refresh()));

    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(nameof(Kind), typeof(ChipKind), typeof(Chip), new PropertyMetadata(ChipKind.Neutral, (d, _) => ((Chip)d).Refresh()));

    public Chip()
    {
        InitializeComponent();
        ActualThemeChanged += (_, _) => Refresh();
        Loaded += (_, _) => Refresh();
    }

    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public ChipKind Kind { get => (ChipKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }

    private void Refresh()
    {
        Label.Text = Text;
        var (fg, bg) = Kind switch
        {
            ChipKind.Accent => ("ClAccent", "ClAccentTint"),
            ChipKind.Cyan => ("ClCyan", "ClCyanTint"),
            ChipKind.Ok => ("ClOk", "ClOkTint"),
            ChipKind.Warn => ("ClWarn", "ClWarnTint"),
            ChipKind.Danger => ("ClDanger", "ClDangerTint"),
            _ => ("ClTextSecondary", "ClSurface3"),
        };
        Label.Foreground = ThemeBrushes.Get(fg, ActualTheme);
        Root.Background = ThemeBrushes.Get(bg, ActualTheme);
        Root.BorderBrush = ThemeBrushes.Get(fg, ActualTheme);
    }
}

public static class ThemeBrushes
{
    /// <summary>Looks up a brand brush for the given theme so code can color things the same as XAML does.</summary>
    public static Brush Get(string name, ElementTheme theme)
    {
        var key = new Windows.UI.ViewManagement.AccessibilitySettings().HighContrast ? "HighContrast"
            : theme == ElementTheme.Light ? "Light" : "Default";
        foreach (var dict in Application.Current.Resources.MergedDictionaries)
        {
            if (dict.ThemeDictionaries.TryGetValue(key, out var found) && found is ResourceDictionary rd && rd.TryGetValue(name + "Brush", out var brush))
                return (Brush)brush;
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }
}
