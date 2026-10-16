using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Clarion.App.Services;

/// <summary>The small pieces of text that dialogs built in code repeat, so they look the same wherever they appear.</summary>
public static class Ui
{
    /// <summary>A line of secondary text: small, a little faded, wrapping.</summary>
    public static TextBlock Note(string text, double maxWidth = double.PositiveInfinity) => new()
    {
        Text = text,
        TextWrapping = TextWrapping.Wrap,
        FontSize = 12,
        Opacity = 0.8,
        MaxWidth = maxWidth,
    };

    /// <summary>A line of something exact, such as a registry value or a command, in the fixed width font.</summary>
    public static TextBlock Mono(string text)
    {
        var line = Note(text);
        line.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Cascadia Mono");
        return line;
    }
}
