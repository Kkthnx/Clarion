using Clarion.Core.Layout;

namespace Clarion.Tests;

public sealed class SearchMatchTests
{
    private static readonly string[] Mouse = ["Turn off mouse acceleration", "Stop the pointer speeding up as you move the mouse faster.", "Input"];

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("mouse")]
    [InlineData("MOUSE")]
    [InlineData("accel mouse")]
    [InlineData("mouse   pointer")]
    [InlineData("input accel")]
    public void A_setting_matches_when_every_word_is_found_somewhere_in_any_order(string query) =>
        Assert.True(SearchMatch.Matches(query, Mouse));

    [Theory]
    [InlineData("mouse keyboard")]
    [InlineData("telemetry")]
    public void A_word_that_is_nowhere_stops_the_match(string query) =>
        Assert.False(SearchMatch.Matches(query, Mouse));

    [Theory]
    [InlineData("\\")]
    [InlineData("[")]
    [InlineData("(((")]
    [InlineData(".*")]
    public void Characters_that_mean_something_elsewhere_are_just_text(string query) =>
        Assert.False(SearchMatch.Matches(query, Mouse));

    [Fact]
    public void An_empty_search_with_no_filter_has_a_plain_message() =>
        Assert.Equal("Nothing to show here.", SearchMatch.EmptyMessage("", false, false));

    [Fact]
    public void A_search_with_no_answer_says_what_was_looked_for_and_what_to_try()
    {
        var text = SearchMatch.EmptyMessage("  xyz ", true, true);

        Assert.Contains("\"xyz\"", text, StringComparison.Ordinal);
        Assert.Contains("All settings", text, StringComparison.Ordinal);
        Assert.Contains("Expert mode", text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_filter_with_nothing_in_it_says_so() =>
        Assert.Contains("filter", SearchMatch.EmptyMessage("", true, false), StringComparison.Ordinal);
}
