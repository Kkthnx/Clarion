using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Clarion.App.Controls;

/// <summary>Lays children out in rows and starts a new row when the next one would not fit, so a long set of pills is never cut off.</summary>
public sealed class WrapPanel : Panel
{
    public double HorizontalSpacing { get; set; } = 6;
    public double VerticalSpacing { get; set; } = 6;

    protected override Size MeasureOverride(Size available)
    {
        var x = 0.0;
        var rowHeight = 0.0;
        var width = 0.0;
        var height = 0.0;
        var any = false;

        foreach (var child in Children)
        {
            child.Measure(new Size(available.Width, double.PositiveInfinity));
            var size = child.DesiredSize;
            if (any && x + size.Width > available.Width)
            {
                width = Math.Max(width, x - HorizontalSpacing);
                height += rowHeight + VerticalSpacing;
                x = 0;
                rowHeight = 0;
            }
            x += size.Width + HorizontalSpacing;
            rowHeight = Math.Max(rowHeight, size.Height);
            any = true;
        }

        if (!any) return new Size(0, 0);
        width = Math.Max(width, x - HorizontalSpacing);
        return new Size(double.IsInfinity(available.Width) ? width : Math.Min(width, available.Width), height + rowHeight);
    }

    protected override Size ArrangeOverride(Size final)
    {
        var x = 0.0;
        var y = 0.0;
        var rowHeight = 0.0;
        var any = false;

        foreach (var child in Children)
        {
            var size = child.DesiredSize;
            if (any && x + size.Width > final.Width)
            {
                y += rowHeight + VerticalSpacing;
                x = 0;
                rowHeight = 0;
            }
            child.Arrange(new Rect(x, y, size.Width, size.Height));
            x += size.Width + HorizontalSpacing;
            rowHeight = Math.Max(rowHeight, size.Height);
            any = true;
        }
        return final;
    }
}
