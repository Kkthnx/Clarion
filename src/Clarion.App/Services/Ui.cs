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

    /// <summary>A row with a wide first column and a second that is as wide as its content, for a name and the control or note beside it.</summary>
    public static Grid StarAutoRow(double spacing)
    {
        var row = new Grid { ColumnSpacing = spacing };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        return row;
    }

    /// <summary>A line of something exact, such as a registry value or a command, in the fixed width font.</summary>
    public static TextBlock Mono(string text)
    {
        var line = Note(text);
        line.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Cascadia Mono");
        return line;
    }
}
