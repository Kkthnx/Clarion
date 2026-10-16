namespace Clarion.Core.Layout;

/// <summary>
/// How many cards sit side by side in a list, and how a list is cut into rows of that many. Sparkle's Tweaks page does the same with a grid
/// that goes from one to four columns at fixed window widths. Here the number comes from the width the list really has, since the window can
/// be any size and the navigation takes a share of it, and a card never gets less room than it needs for its name, summary and switch.
/// </summary>
public static class CardColumns
{
    /// <summary>About what one card needs, in device independent pixels, to show its name, summary and switch without cramping.</summary>
    public const double MinCardWidth = 480;

    public const int MaxColumns = 3;

    public const double Gap = 8;

    /// <summary>One column in the compact list and in a narrow window, otherwise as many as fit, up to <see cref="MaxColumns"/>.</summary>
    public static int For(double listWidth, bool compact)
    {
        if (compact || listWidth <= 0) return 1;
        var fit = (int)Math.Floor((listWidth + Gap) / (MinCardWidth + Gap));
        return Math.Clamp(fit, 1, MaxColumns);
    }

    /// <summary>The items in order, <paramref name="columns"/> to a row. The last row can be shorter.</summary>
    public static IReadOnlyList<IReadOnlyList<T>> Rows<T>(IReadOnlyList<T> items, int columns)
    {
        columns = Math.Clamp(columns, 1, MaxColumns);
        var rows = new List<IReadOnlyList<T>>((items.Count + columns - 1) / columns);
        for (var i = 0; i < items.Count; i += columns) rows.Add(items.Skip(i).Take(columns).ToList());
        return rows;
    }
}
