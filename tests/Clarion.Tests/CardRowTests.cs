using Clarion.Core.Layout;

namespace Clarion.Tests;

public sealed class CardRowTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(300, 1)]
    [InlineData(789, 1)]      // the default window: two cards would each get about 390
    [InlineData(967, 1)]
    [InlineData(968, 2)]      // two cards of 480 and the gap between them
    [InlineData(1251, 2)]     // a maximized window on a laptop
    [InlineData(1455, 2)]
    [InlineData(1456, 3)]
    [InlineData(3000, 3)]     // never more than three
    public void The_number_of_columns_follows_the_width_the_list_really_has(double width, int columns) =>
        Assert.Equal(columns, CardColumns.For(width, compact: false));

    [Theory]
    [InlineData(500)]
    [InlineData(1251)]
    [InlineData(3000)]
    public void The_compact_list_is_always_one_line_per_setting(double width) =>
        Assert.Equal(1, CardColumns.For(width, compact: true));

    [Fact]
    public void A_card_never_gets_less_room_than_it_needs_whatever_the_width()
    {
        for (var width = 100; width <= 4000; width += 7)
        {
            var columns = CardColumns.For(width, compact: false);
            var card = (width - CardColumns.Gap * (columns - 1)) / columns;
            if (columns > 1) Assert.True(card >= CardColumns.MinCardWidth, $"{width} wide gives {columns} columns of {card}");
        }
    }

    [Theory]
    [InlineData(0, 2, 0)]
    [InlineData(1, 2, 1)]
    [InlineData(4, 2, 2)]
    [InlineData(5, 2, 3)]
    [InlineData(7, 3, 3)]
    [InlineData(9, 3, 3)]
    [InlineData(5, 1, 5)]
    public void A_list_is_cut_into_rows_of_the_right_size_and_keeps_every_item_in_order(int count, int columns, int rows)
    {
        var items = Enumerable.Range(0, count).ToList();
        var cut = CardColumns.Rows(items, columns);

        Assert.Equal(rows, cut.Count);
        Assert.Equal(items, cut.SelectMany(r => r).ToList());
        Assert.All(cut.Take(Math.Max(0, cut.Count - 1)), r => Assert.Equal(columns, r.Count));
        Assert.All(cut, r => Assert.InRange(r.Count, 1, columns));
    }

    [Fact]
    public void More_columns_than_the_most_are_treated_as_the_most()
    {
        var cut = CardColumns.Rows(Enumerable.Range(0, 10).ToList(), 9);
        Assert.Equal(CardColumns.MaxColumns, cut[0].Count);
    }
}
