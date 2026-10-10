using System.Text.RegularExpressions;

namespace Clarion.Tests;

/// <summary>
/// When Windows is in a high contrast theme, the app must use the system's colours and nothing of its own. A colour that exists in the
/// normal themes but is missing here has no value to fall back on, and a hard coded one can be unreadable on the user's chosen contrast
/// theme. The file is read as text, so a colour added later cannot be forgotten.
/// </summary>
public sealed partial class ThemeCompletenessTests
{
    [GeneratedRegex("""<ResourceDictionary x:Key="(?<name>Default|Light|HighContrast)">(?<body>.*?)</ResourceDictionary>""", RegexOptions.Singleline)]
    private static partial Regex Dictionaries();

    private static Dictionary<string, string> Themes()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        var xaml = File.ReadAllText(Path.Combine(dir!.FullName, "src", "Clarion.App", "Theme.xaml"));
        return Dictionaries().Matches(xaml).ToDictionary(m => m.Groups["name"].Value, m => m.Groups["body"].Value);
    }

    private static IReadOnlyList<string> Keys(string body, bool brandOnly) =>
        Regex.Matches(body, "x:Key=\"([^\"]+)\"").Select(m => m.Groups[1].Value).Where(k => !brandOnly || k.StartsWith("Cl", StringComparison.Ordinal)).ToList();

    [Fact]
    public void All_three_themes_exist()
    {
        var themes = Themes();
        Assert.Contains("Default", themes.Keys);
        Assert.Contains("Light", themes.Keys);
        Assert.Contains("HighContrast", themes.Keys);
    }

    [Fact]
    public void Every_colour_of_the_normal_themes_has_a_high_contrast_value()
    {
        var themes = Themes();
        var high = Keys(themes["HighContrast"], brandOnly: true);

        Assert.All(new[] { "Default", "Light" }, name =>
            Assert.All(Keys(themes[name], brandOnly: true), k => Assert.Contains(k, high)));
    }

    [Fact]
    public void High_contrast_uses_only_the_colours_windows_gives_it()
    {
        var body = Themes()["HighContrast"];

        Assert.DoesNotMatch("""Color="#""", body);
        Assert.DoesNotMatch("""<Color\b""", body);
        foreach (Match m in Regex.Matches(body, "ResourceKey=\"([^\"]+)\"")) Assert.StartsWith("SystemColor", m.Groups[1].Value);
    }

    [Fact]
    public void The_custom_scroll_bar_colours_stay_out_of_high_contrast_so_windows_draws_them()
    {
        var themes = Themes();

        Assert.All(Keys(themes["HighContrast"], brandOnly: false), k => Assert.False(k.StartsWith("ScrollBar", StringComparison.Ordinal), k));
        Assert.Contains(Keys(themes["Default"], brandOnly: false), k => k.StartsWith("ScrollBar", StringComparison.Ordinal));
    }
}
