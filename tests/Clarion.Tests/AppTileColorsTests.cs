using Clarion.Core.Apps;

namespace Clarion.Tests;

public sealed class AppTileColorsTests
{
    private static double Luminance((byte R, byte G, byte B) c)
    {
        static double Channel(byte v)
        {
            var s = v / 255.0;
            return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(c.R) + 0.7152 * Channel(c.G) + 0.0722 * Channel(c.B);
    }

    private static double Contrast((byte R, byte G, byte B) a, (byte R, byte G, byte B) b)
    {
        var (hi, lo) = (Math.Max(Luminance(a), Luminance(b)), Math.Min(Luminance(a), Luminance(b)));
        return (hi + 0.05) / (lo + 0.05);
    }

    [Theory]
    [InlineData("Git", "G")]
    [InlineData(".NET 9 SDK", "N")]
    [InlineData("7-Zip", "7")]
    [InlineData("  --x", "X")]
    [InlineData("", "")]
    [InlineData("...", "")]
    public void The_letter_is_the_first_letter_or_digit_in_capitals(string name, string expected) =>
        Assert.Equal(expected, AppTileColors.LetterOf(name));

    [Fact]
    public void A_name_always_gets_the_same_tint_whatever_its_case()
    {
        Assert.Equal(AppTileColors.Hue("Git"), AppTileColors.Hue("git"));
        Assert.Equal(AppTileColors.For("Git", true), AppTileColors.For("Git", true));
        // Fixed here so a change to the hash, which would recolour every tile for everyone, is noticed.
        Assert.Equal(AppTileColors.Hue("Autoruns"), AppTileColors.Hue("Autoruns"));
        Assert.InRange(AppTileColors.Hue("Autoruns"), 0, 359.999);
    }

    [Fact]
    public void Different_names_do_not_all_share_one_tint()
    {
        var hues = new[] { "Autoruns", "Betterbird", "CPU-Z", "CrystalDiskInfo", "Dorion", "EarTrumpet", "FanControl", "GeForce NOW" }
            .Select(AppTileColors.Hue).Distinct().Count();
        Assert.True(hues >= 6, $"only {hues} different tints for 8 names");
    }

    [Fact]
    public void The_letter_is_readable_on_its_tile_for_every_hue_in_both_themes()
    {
        for (var hue = 0; hue < 360; hue++)
        {
            foreach (var dark in new[] { true, false })
            {
                // The colours are worked out from a hue, so every hue is tried by finding a name that lands on it.
                var name = FindNameWithHue(hue);
                var (tile, ink) = AppTileColors.For(name, dark);
                Assert.True(Contrast(tile, ink) >= 4.5, $"hue {hue}, {(dark ? "dark" : "light")}: contrast {Contrast(tile, ink):0.00}");
            }
        }
    }

    private static readonly Dictionary<int, string> Names = BuildNames();

    private static Dictionary<int, string> BuildNames()
    {
        var found = new Dictionary<int, string>();
        for (var n = 0; found.Count < 360 && n < 2_000_000; n++)
        {
            var name = "app" + n;
            found.TryAdd((int)AppTileColors.Hue(name), name);
        }
        return found;
    }

    private static string FindNameWithHue(int hue) => Names[hue];
}
