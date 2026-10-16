using Clarion.Core.Layout;

namespace Clarion.App.Services;

/// <summary>
/// Up to three settings shown side by side in one list row, for a window wide enough to hold more than one card. The row is as tall as its
/// tallest card, so every card keeps its natural height, which a grid of equal cells cannot do when one card has an extra line, and the list
/// still creates only the rows on screen. The second and third are null when the row is not full, as at the end of the list.
/// </summary>
public sealed class CardRow(TweakItem first, TweakItem? second, TweakItem? third)
{
    public TweakItem First { get; } = first;

    public TweakItem? Second { get; } = second;

    public TweakItem? Third { get; } = third;

    public bool HasSecond => Second is not null;

    public bool HasThird => Third is not null;

    /// <summary>The settings, <paramref name="columns"/> to a row, in order.</summary>
    public static IReadOnlyList<CardRow> Of(IReadOnlyList<TweakItem> items, int columns) =>
        CardColumns.Rows(items, columns)
            .Select(r => new CardRow(r[0], r.Count > 1 ? r[1] : null, r.Count > 2 ? r[2] : null))
            .ToList();
}
