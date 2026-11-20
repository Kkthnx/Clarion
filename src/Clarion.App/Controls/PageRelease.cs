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
        var holders = new List<ContentControl>();
        Collect(root, lists, holders);
        // A card in a row of two or three is a ContentControl that holds the setting itself, not a list, so emptying the list left its
        // bindings on the setting. Measured on Debloat in three columns: one more handler on a row for every visit. They are emptied too.
        // A compiled binding lets go of its row only when its data context changes, and emptying the holder drops the template without
        // that. So the data context of what the holder shows is cleared first, then the holder.
        foreach (var holder in holders)
        {
            if (TemplateRoot(holder) is { } shown) shown.DataContext = null;
            holder.Content = null;
        }
        // Children come after their parents in the walk, so the reversed order empties the innermost lists first.
        for (var i = lists.Count - 1; i >= 0; i--) lists[i].ItemsSource = null;
    }

    /// <summary>The element the holder's template made for its content, found under the ContentPresenter that shows it.</summary>
    private static FrameworkElement? TemplateRoot(ContentControl holder)
    {
        DependencyObject node = holder;
        while (VisualTreeHelper.GetChildrenCount(node) > 0)
        {
            node = VisualTreeHelper.GetChild(node, 0);
            if (node is ContentPresenter presenter)
                return VisualTreeHelper.GetChildrenCount(presenter) > 0 ? VisualTreeHelper.GetChild(presenter, 0) as FrameworkElement : null;
        }
        return null;
    }

    private static void Collect(DependencyObject node, List<ItemsControl> lists, List<ContentControl> holders)
    {
        if (node is ItemsControl list) lists.Add(list);
        // Only a holder of data. A button's words or a check box's text are its own and go with the page.
        // A list's own item containers are left to the list, which lets go of them when it is emptied.
        else if (node is ContentControl { Content: not null and not UIElement and not string } holder and not Microsoft.UI.Xaml.Controls.Primitives.SelectorItem)
            holders.Add(holder);
        var count = VisualTreeHelper.GetChildrenCount(node);
        for (var i = 0; i < count; i++) Collect(VisualTreeHelper.GetChild(node, i), lists, holders);
    }
}
