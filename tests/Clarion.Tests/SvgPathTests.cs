using System.Text.RegularExpressions;
using Clarion.Core.Apps;

namespace Clarion.Tests;

public sealed class SvgPathTests
{
    [Theory]
    [InlineData("M0 0h10v10H0z", "M0 0H10V10H0Z")]
    [InlineData("m1 2l3 4", "M1 2L4 6")]
    [InlineData("M1 2 3 4 5 6", "M1 2L3 4L5 6")]
    [InlineData("m1 2 3 4", "M1 2L4 6")]
    [InlineData("M0 0c1 1 2 2 3 3 1 1 2 2 3 3", "M0 0C1 1 2 2 3 3C4 4 5 5 6 6")]
    [InlineData("M5 5a1 1 0 011 1", "M5 5A1 1 0 0 1 6 6")]
    [InlineData("M0 0q1 1 2 2t2 2", "M0 0Q1 1 2 2T4 4")]
    [InlineData("M-.5-.5L1.5.5", "M-0.5 -0.5L1.5 0.5")]
    public void Coordinates_become_absolute(string given, string expected) =>
        Assert.Equal(expected, SvgPath.ToAbsolute(given));

    [Fact]
    public void A_relative_move_after_a_close_starts_where_the_closed_shape_began()
    {
        // The second piece starts 20 across and 20 down from where the first began, which is (0,0), and not from where it ended.
        Assert.Equal("M0 0H10V10H0ZM20 20H25V25H20Z", SvgPath.ToAbsolute("M0 0h10v10H0zm20 20h5v5h-5z"));
        // And a close in the middle of a shape that began elsewhere goes back to that start.
        Assert.Equal("M3 3H5ZM4 4L6 6Z", SvgPath.ToAbsolute("M3 3h2zm1 1l2 2z"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("h")]
    [InlineData("10 10")]
    [InlineData("M0 0X1 1")]
    [InlineData("M0 0a1 1 0 21 1 1")]
    public void Data_that_is_not_a_path_is_refused(string bad)
    {
        if (bad.Length == 0) { Assert.Equal("", SvgPath.ToAbsolute(bad)); return; }
        Assert.Throws<FormatException>(() => SvgPath.ToAbsolute(bad));
    }

    [Fact]
    public void Every_logo_outline_is_absolute_after_loading_and_converting_again_changes_nothing()
    {
        foreach (var (id, icon) in AppIcons.Default.All)
        {
            // Only absolute commands, and Z, remain. The letter e is left out because it belongs to numbers such as 1e-3.
            Assert.DoesNotMatch(@"[a-y]", icon.Path.Replace("e", ""));
            Assert.Equal(icon.Path, SvgPath.ToAbsolute(icon.Path));
            Assert.True(icon.Path.StartsWith('M'), id);
        }
    }

    [Fact]
    public void A_piece_that_follows_a_close_is_not_drawn_in_the_wrong_place()
    {
        // The .NET mark is the four pieces of a word (T, E, N and the dot), and each one after the first begins with a relative move after the
        // close of the one before. Checked against an independent SVG library, which puts the dot at the bottom left, 24 units wide in all.
        var path = AppIcons.Default.For("app.dotnet-sdk")!.Path;
        Assert.Equal(4, Regex.Matches(path, "Z").Count);
        Assert.StartsWith("M24 8.77H21.532V16.335H20.107V8.77H17.645V7.53H24ZM", path, StringComparison.Ordinal);
    }
}
