using System.Text.RegularExpressions;

namespace Clarion.Tests;

/// <summary>
/// Text must reach the WCAG AA contrast of 4.5 to 1 against what it sits on, in both themes. Chips and small labels are well under the
/// size where 3 to 1 would do. The colours are read from the theme file itself, so a new colour cannot slip below the line.
/// </summary>
public sealed partial class ThemeContrastTests
{
    private const double Minimum = 4.5;

    [GeneratedRegex("""<ResourceDictionary x:Key="(?<name>Default|Light)">(?<body>.*?)</ResourceDictionary>""", RegexOptions.Singleline)]
    private static partial Regex Dictionaries();

    [GeneratedRegex("""<Color x:Key="Cl(?<key>\w+)Color">#(?<hex>[0-9A-Fa-f]{6})</Color>""")]
    private static partial Regex Colors();

    private static string ThemeFile()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src", "Clarion.App", "Theme.xaml");
    }

    private static Dictionary<string, Dictionary<string, string>> Themes()
    {
        var xaml = File.ReadAllText(ThemeFile());
        var result = new Dictionary<string, Dictionary<string, string>>();
        foreach (Match dict in Dictionaries().Matches(xaml))
        {
            var colors = new Dictionary<string, string>();
            foreach (Match c in Colors().Matches(dict.Groups["body"].Value)) colors[c.Groups["key"].Value] = c.Groups["hex"].Value;
            result[dict.Groups["name"].Value] = colors;
        }
        return result;
    }

    private static double Luminance(string hex)
    {
        double Channel(int at)
        {
            var v = Convert.ToInt32(hex.Substring(at, 2), 16) / 255.0;
            return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(0) + 0.7152 * Channel(2) + 0.0722 * Channel(4);
    }

    private static double Contrast(string a, string b)
    {
        var (la, lb) = (Luminance(a), Luminance(b));
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    [Fact]
    public void Both_themes_are_found_with_the_colours_the_checks_need()
    {
        var themes = Themes();
        Assert.Equal(["Default", "Light"], themes.Keys.Order().ToArray());
        foreach (var colors in themes.Values)
            foreach (var key in new[] { "Base", "Surface1", "Surface2", "Surface3", "TextPrimary", "TextSecondary", "TextMuted", "AccentText", "CyanText", "OkText", "WarnText", "DangerText", "AccentTint", "CyanTint", "OkTint", "WarnTint", "DangerTint" })
                Assert.True(colors.ContainsKey(key), $"missing {key}");
    }

    [Fact]
    public void Body_text_reaches_the_minimum_on_every_surface_in_both_themes()
    {
        var low = new List<string>();
        foreach (var (theme, colors) in Themes())
            foreach (var text in new[] { "TextPrimary", "TextSecondary", "TextMuted" })
                foreach (var surface in new[] { "Base", "Surface1", "Surface2", "Surface3" })
                {
                    var ratio = Contrast(colors[text], colors[surface]);
                    if (ratio < Minimum) low.Add($"{theme}: {text} on {surface} is {ratio:0.00}");
                }
        Assert.Empty(low);
    }

    [Fact]
    public void Status_text_reaches_the_minimum_on_its_own_tint_and_on_the_page_surfaces_in_both_themes()
    {
        var low = new List<string>();
        foreach (var (theme, colors) in Themes())
            foreach (var status in new[] { "Accent", "Cyan", "Ok", "Warn", "Danger" })
                foreach (var background in new[] { $"{status}Tint", "Base", "Surface1", "Surface2" })
                {
                    var ratio = Contrast(colors[$"{status}Text"], colors[background]);
                    if (ratio < Minimum) low.Add($"{theme}: {status}Text on {background} is {ratio:0.00}");
                }
        Assert.Empty(low);
    }
}
