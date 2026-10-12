using System.Text.Json;
using System.Text.RegularExpressions;
using Clarion.Core.Catalog;
using Clarion.Core.Reports;

namespace Clarion.Tests;

public sealed partial class CatalogSiteTests
{
    private static readonly SiteSource Source = SiteSource.FromCatalog("test");
    private static readonly IReadOnlyDictionary<string, string> Site = CatalogSite.Build(Source);

    private static IEnumerable<string> Pages => Site.Keys.Where(k => k.EndsWith(".html", StringComparison.Ordinal));

    [GeneratedRegex("<article class=\"item\" id=\"([^\"]+)\"")]
    private static partial Regex ArticleId();

    [GeneratedRegex("(?:href|src)=\"([^\"]+)\"")]
    private static partial Regex Link();

    [Fact]
    public void Every_setting_repair_job_and_cleanup_row_is_on_the_site_once()
    {
        var expected = Source.Tweaks.Select(t => t.Id).Concat(Source.Actions.Select(a => a.Id)).Concat(Source.Cleanups.Select(c => c.Id)).ToList();
        var found = Pages.SelectMany(p => ArticleId().Matches(Site[p]).Select(m => m.Groups[1].Value)).ToList();
        Assert.Equal(expected.Order(StringComparer.Ordinal), found.Order(StringComparer.Ordinal));
        Assert.Equal(found.Count, found.Distinct().Count());
    }

    [Fact]
    public void Every_link_on_the_site_goes_somewhere()
    {
        var anchors = Pages.ToDictionary(p => p, p => ArticleId().Matches(Site[p]).Select(m => m.Groups[1].Value).ToHashSet(StringComparer.Ordinal));
        var broken = new List<string>();
        foreach (var page in Pages)
        {
            foreach (Match m in Link().Matches(Site[page]))
            {
                var href = System.Net.WebUtility.HtmlDecode(m.Groups[1].Value);
                if (href.StartsWith("https://", StringComparison.Ordinal) || href.StartsWith('#')) continue;
                var (file, anchor) = href.Contains('#') ? (href[..href.IndexOf('#')], href[(href.IndexOf('#') + 1)..]) : (href, "");
                // The images are copied in by the tool that writes the folder, so only pages and generated assets are checked here.
                if (file is "assets/icon.png" or "assets/banner.png") continue;
                if (!Site.ContainsKey(file)) broken.Add($"{page}: {href} (no such file)");
                else if (anchor.Length > 0 && !(anchors.TryGetValue(file, out var set) && set.Contains(anchor))) broken.Add($"{page}: {href} (no such entry)");
            }
        }
        Assert.Empty(broken);
    }

    [Fact]
    public void Preset_sections_exist_and_list_real_settings()
    {
        var presets = Site["presets.html"];
        foreach (var (key, _, _) in PresetInfo.All) Assert.Contains($"id=\"{key}\"", presets, StringComparison.Ordinal);
        var known = Source.Tweaks.Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        Assert.All(Source.Presets.Values.SelectMany(v => v), id => Assert.Contains(id, known));
    }

    [Fact]
    public void Text_from_the_catalog_is_escaped()
    {
        var tweak = Source.Tweaks[0] with { Name = "<b>Bold</b> & \"quoted\"", Summary = "<script>alert(1)</script>" };
        var site = CatalogSite.Build(Source with { Tweaks = [tweak, .. Source.Tweaks.Skip(1)] });
        var all = string.Concat(site.Where(p => p.Key.EndsWith(".html", StringComparison.Ordinal)).Select(p => p.Value));
        Assert.DoesNotContain("<script>alert", all, StringComparison.Ordinal);
        Assert.DoesNotContain("<b>Bold", all, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;Bold&lt;/b&gt;", all, StringComparison.Ordinal);
    }

    [Fact]
    public void No_page_loads_anything_from_another_site_except_links()
    {
        foreach (var page in Pages)
        {
            Assert.DoesNotMatch("<(script|link|img)[^>]+(src|href)=\"https?://", Site[page]);
        }
    }

    [Fact]
    public void Pages_use_no_dashes_in_the_text_the_site_adds_or_the_catalog_supplies()
    {
        foreach (var page in Pages) Assert.DoesNotMatch("[–—]", Site[page]);
    }

    [Fact]
    public void Every_page_has_a_title_a_description_and_one_main_heading()
    {
        foreach (var page in Pages)
        {
            var html = Site[page];
            Assert.Matches("<title>[^<]+</title>", html);
            Assert.Matches("<meta name=\"description\" content=\"[^\"]+\">", html);
            Assert.Single(Regex.Matches(html, "<h1[ >]"));
        }
    }

    [Fact]
    public void The_search_index_has_one_row_per_entry()
    {
        using var doc = JsonDocument.Parse(Site["assets/search-index.json"]);
        var ids = doc.RootElement.EnumerateArray().Select(r => r.GetProperty("i").GetString()!).ToList();
        Assert.Equal(Source.Tweaks.Count + Source.Actions.Count + Source.Cleanups.Count, ids.Count);
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void The_apps_group_is_called_Apps_like_the_window_does() => Assert.True(Site.ContainsKey("apps.html"));

    [Fact]
    public void The_stylesheet_uses_the_windows_colors_for_dark_and_light()
    {
        var xaml = File.ReadAllText(Path.Combine(RepositoryRoot(), "src", "Clarion.App", "Theme.xaml"));
        var lightAt = xaml.IndexOf("x:Key=\"Light\"", StringComparison.Ordinal);
        var highAt = xaml.IndexOf("x:Key=\"HighContrast\"", StringComparison.Ordinal);
        var app = new Dictionary<string, Dictionary<string, string>>
        {
            ["dark"] = Colors(xaml[xaml.IndexOf("x:Key=\"Default\"", StringComparison.Ordinal)..lightAt]),
            ["light"] = Colors(xaml[lightAt..highAt]),
        };
        var css = Site["assets/site.css"];
        var blocks = new Dictionary<string, string>
        {
            ["dark"] = Block(css, ":root{"),
            ["light"] = Block(css, ":root[data-theme=\"light\"]{"),
        };
        var pairs = new Dictionary<string, string>
        {
            ["base"] = "ClBase", ["sidebar"] = "ClSidebar", ["s1"] = "ClSurface1", ["s2"] = "ClSurface2", ["s3"] = "ClSurface3",
            ["border"] = "ClBorder", ["divider"] = "ClDivider", ["text"] = "ClTextPrimary", ["text2"] = "ClTextSecondary", ["muted"] = "ClTextMuted",
            ["accent"] = "ClAccent", ["accent-tint"] = "ClAccentTint", ["on-accent"] = "ClOnAccent", ["accent-text"] = "ClAccentText",
            ["ok"] = "ClOkText", ["ok-tint"] = "ClOkTint", ["warn"] = "ClWarnText", ["warn-tint"] = "ClWarnTint",
            ["danger-text"] = "ClDangerText", ["danger-tint"] = "ClDangerTint",
        };
        foreach (var theme in app.Keys)
        {
            var site = Variables(blocks[theme]);
            foreach (var (name, key) in pairs)
                Assert.True(site.TryGetValue(name, out var value) && string.Equals(value, app[theme][key], StringComparison.OrdinalIgnoreCase),
                    $"{theme} --{name} should be {app[theme][key]} like {key} in Theme.xaml, but is {(site.TryGetValue(name, out var v) ? v : "missing")}.");
        }
    }

    private static Dictionary<string, string> Colors(string xaml) =>
        Regex.Matches(xaml, "<Color x:Key=\"(\\w+)Color\">(#[0-9A-Fa-f]{6})</Color>").ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);

    private static string Block(string css, string selector)
    {
        var start = css.IndexOf(selector, StringComparison.Ordinal) + selector.Length;
        return css[start..css.IndexOf('}', start)];
    }

    private static Dictionary<string, string> Variables(string block) =>
        Regex.Matches(block, "--([a-z0-9-]+):(#[0-9A-Fa-f]{6})").ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);

    private static string RepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) return dir.FullName;
        throw new InvalidOperationException("The repository root was not found.");
    }
}
