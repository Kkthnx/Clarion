using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Clarion.App.Controls;

/// <summary>
/// Lets go of the rows a page was showing when the page is left. A page is made again on every visit, but the rows it showed, such as a
/// cleanup row or a setting, live as long as the program. A compiled binding to such a row listens to its PropertyChanged, and when the
/// page is only dropped the listener stays on the row. Every visit then adds one more, and each keeps that visit's elements alive in native
/// memory. Measured on the Clean up page: 40 visits grew the program by 55 MB, and the handlers on one row went from 3 to 8 in 8 visits.
/// Emptying the lists lets the bindings stop listening.
/// </summary>
public static class PageRelease
{
    /// <summary>Empties every list under <paramref name="root"/>, the inner ones first, so the rows of each are released.</summary>
    public static void Lists(DependencyObject root)
    {
        var lists = new List<ItemsControl>();
        Collect(root, lists);
        // Children come after their parents in the walk, so the reversed order empties the innermost lists first.
        for (var i = lists.Count - 1; i >= 0; i--) lists[i].ItemsSource = null;
    }

    private static void Collect(DependencyObject node, List<ItemsControl> lists)
    {
        if (node is ItemsControl list) lists.Add(list);
        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var i = 0; i < count; i++) Collect(VisualTreeHelper.GetChild(node, i), lists);
    }
}
