using System.Net;
using System.Text;
using System.Text.Json;
using Clarion.Core.Actions;
using Clarion.Core.Catalog;
using Clarion.Core.Cleanup;
using Clarion.Core.Model;

namespace Clarion.Core.Reports;

/// <summary>Everything the documentation site is written from. It is the same catalog the app runs on.</summary>
public sealed record SiteSource(
    IReadOnlyList<Tweak> Tweaks,
    IReadOnlyList<ActionDef> Actions,
    IReadOnlyList<CleanTarget> Cleanups,
    IReadOnlyDictionary<string, IReadOnlyList<string>> Presets,
    string Version)
{
    public static SiteSource FromCatalog(string version) =>
        new(CatalogLoader.LoadEmbedded(), CatalogLoader.LoadActions(), CatalogLoader.LoadCleanup(), CatalogLoader.LoadPresets(), version);
}

/// <summary>
/// The documentation site for GitHub Pages: one page for every group of settings, the presets, the repair jobs and the cleanup rows,
/// and a search index. It is written from the catalog, so it cannot disagree with what the app does, and a test checks that every
/// setting is on it exactly once. The files are plain HTML with one stylesheet and one script, and the script is only needed for
/// search and filtering, so every page reads fine without it.
/// </summary>
public static class CatalogSite
{
    public const string BaseUrl = "https://kkthnx.github.io/Clarion/";
    private const string Repo = "https://github.com/Kkthnx/Clarion";

    /// <summary>The order the app lists its groups in. A group not named here follows in alphabetical order.</summary>
    private static readonly string[] CategoryOrder = ["Privacy", "Debloat", "Install", "Appearance", "Explorer", "Features", "Input", "Network", "Power", "Security", "System", "Updates"];

    private sealed record Entry(
        string Id, string Name, string Topic, string Summary, string Advice, Recommendation Recommendation, IReadOnlyList<string> Facts,
        string? What, string Benefit, string Risk, RiskLevel RiskLevel, string? Evidence, IReadOnlyList<string> Notes,
        string ChangesTitle, IReadOnlyList<string> Changes, IReadOnlyList<string> Undo, IReadOnlyList<string> Sources, IReadOnlyList<string> Provenance);

    private sealed record Group(string File, string Title, string Blurb, IReadOnlyList<Entry> Entries);

    /// <summary>Every page of the site, keyed by its path, as text. The images are copied in by the tool that writes the folder.</summary>
    public static IReadOnlyDictionary<string, string> Build(SiteSource source)
    {
        var groups = CategoryGroups(source.Tweaks);
        var repair = new Group("repair.html", "Repair",
            "One time jobs that fix Windows itself. They are not settings, so they have no on or off state and nothing to revert. Output streams live in the app and any job can be stopped.",
            source.Actions.OrderBy(a => a.Name, StringComparer.Ordinal).Select(FromAction).ToList());
        var cleanup = new Group("cleanup.html", "Clean up",
            "Rows in the Clean up list. The app scans first and shows the exact folders and sizes, and nothing is removed until it is ticked and confirmed.",
            source.Cleanups.OrderBy(c => c.Group, StringComparer.Ordinal).ThenBy(c => c.Name, StringComparer.Ordinal).Select(FromCleanTarget).ToList());

        var pages = new List<Group>(groups) { repair, cleanup };
        var nav = new List<NavLink> { new("index.html", "Home", 0), new("presets.html", "Presets", source.Presets.Count) };
        nav.AddRange(pages.Select(g => new NavLink(g.File, g.Title, g.Entries.Count)));

        var files = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["index.html"] = Shell(nav, source.Version, "index.html", "Home", HomeBody(source, groups, repair, cleanup), "Every setting in .Clarion explained: what it does, what you gain, what can go wrong, and how to put it back."),
            ["presets.html"] = Shell(nav, source.Version, "presets.html", "Presets", PresetsBody(source), "The five presets in .Clarion and the exact settings each one queues."),
        };
        foreach (var g in pages)
            files[g.File] = Shell(nav, source.Version, g.File, g.Title, GroupBody(g), $"{g.Title} in .Clarion: {g.Entries.Count} entries, each with what it does, the benefit, the risk and the exact changes.");

        files["assets/search-index.json"] = SearchIndex(pages);
        files["assets/site.css"] = Resource("site.css");
        files["assets/site.js"] = Resource("site.js");
        files["404.html"] = Shell(nav, source.Version, "404.html", "Page not found", "<section class=\"intro\"><h1>Page not found</h1><p>That page does not exist. Try the search box above, or go back to the <a href=\"index.html\">home page</a>.</p></section>", "Page not found.");
        return files;
    }

    // ---- Entries -------------------------------------------------------------------------------------------------------------

    private static List<Group> CategoryGroups(IReadOnlyList<Tweak> tweaks)
    {
        var order = CategoryOrder.ToList();
        return tweaks.GroupBy(t => t.Category)
            .OrderBy(g => order.IndexOf(g.Key) is var i and >= 0 ? i : int.MaxValue).ThenBy(g => g.Key, StringComparer.Ordinal)
            .Select(g =>
            {
                var title = CategoryTitle(g.Key);
                var topics = g.Select(t => t.Topic).Where(x => x.Length > 0).Distinct().OrderBy(x => x, StringComparer.Ordinal).ToList();
                var noun = g.Key == "Install" ? "apps" : "settings";
                var blurb = topics.Count == 0 ? $"{g.Count()} {noun}." : $"{g.Count()} {noun} in {topics.Count} topic{(topics.Count == 1 ? "" : "s")}: {JoinNames(topics)}.";
                if (g.Key == "Install") blurb += " Each one installs an exact winget package. App names and logos belong to their owners and are listed only to say what an entry installs.";
                return new Group(Slug(title) + ".html", title, blurb,
                    g.OrderBy(t => t.Topic, StringComparer.Ordinal).ThenBy(t => t.Name, StringComparer.Ordinal).ThenBy(t => t.Id, StringComparer.Ordinal).Select(FromTweak).ToList());
            }).ToList();
    }

    /// <summary>The catalog calls the apps group Install. The window calls it Apps, and so does this site.</summary>
    private static string CategoryTitle(string category) => category == "Install" ? "Apps" : category;

    private static Entry FromTweak(Tweak t)
    {
        var tip = TweakTooltip.Build(t);
        return new Entry(t.Id, t.Name, t.Topic, t.Summary, t.Advice, t.Recommendation, t.Facts, t.What, t.Benefit, t.Risk, t.RiskLevel, tip.EvidenceLabel,
            tip.Notes, "Exactly what changes", t.Apply.Select(o => o.Describe()).ToList(), t.Undo.Select(o => o.Describe()).ToList(), t.Sources, t.Provenance);
    }

    private static Entry FromAction(ActionDef a)
    {
        var notes = new List<string> { $"Takes about {a.EstimatedMinutes} minute{(a.EstimatedMinutes == 1 ? "" : "s")}." };
        if (a.RestorePoint) notes.Add("A restore point is made first.");
        if (a.NeedsReboot) notes.Add("Restart needed to finish.");
        var steps = a.Steps.Select(s => s.Describe()).ToList();
        steps.AddRange(a.Always.Select(s => "Afterward, always: " + s.Describe()));
        return new Entry(a.Id, a.Name, "", a.Summary, a.Advice, a.Recommendation, a.Facts, a.What, a.Benefit, a.Risk, a.RiskLevel, null, notes,
            "Steps it runs", steps, [], a.Sources, []);
    }

    private static Entry FromCleanTarget(CleanTarget c)
    {
        var notes = new List<string>();
        notes.Add(c.Irreversible ? "Cannot be undone." : "Files are removed, not moved, so check the sizes in the scan first.");
        if (c.DefaultOn) notes.Add("Ticked by default in the list.");
        if (c.ExpertOnly) notes.Add("Shown only in Expert mode.");
        if (c.MinAgeHours > 0) notes.Add($"Only files untouched for {c.MinAgeHours} hours are removed.");
        if (c.Processes.Count > 0) notes.Add($"Skipped while {JoinNames(c.Processes)} is running.");
        if (c.QueueLockedAtRestart) notes.Add("Files that are in use can be queued for removal at the next restart.");
        return new Entry(c.Id, c.Name, c.Group, c.Summary, c.Advice, c.Recommendation, c.Facts, null, c.Benefit, c.Risk, c.RiskLevel, null, notes,
            "Where it looks", c.Rules.Select(DescribeRule).ToList(), [], c.Sources, []);
    }

    private static string DescribeRule(CleanRule rule) => rule switch
    {
        FolderRule f => $"Everything inside {f.Path}. The folder itself is kept.",
        FilePatternRule p => $"Files named {p.Pattern} in {p.Folder}",
        SteamShaderRule => "The shader cache folder in every Steam library",
        WowCacheRule => "The Cache folder of each installed World of Warcraft version",
        RecycleBinRule => "The Recycle Bin",
        EventLogsRule => "Windows event logs. The Security log is never cleared.",
        _ => rule.GetType().Name,
    };

    // ---- Pages ---------------------------------------------------------------------------------------------------------------

    private sealed record NavLink(string File, string Label, int Count);

    private static string Shell(IReadOnlyList<NavLink> nav, string version, string file, string title, string body, string description)
    {
        var sb = new StringBuilder();
        var full = file == "index.html" ? ".Clarion: every setting explained" : $"{title} · .Clarion";
        sb.Append("<!doctype html>\n<html lang=\"en\">\n<head>\n<meta charset=\"utf-8\">\n<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n");
        sb.Append("<meta name=\"color-scheme\" content=\"dark light\">\n");
        sb.Append($"<title>{H(full)}</title>\n<meta name=\"description\" content=\"{H(description)}\">\n");
        sb.Append($"<meta property=\"og:title\" content=\"{H(full)}\">\n<meta property=\"og:description\" content=\"{H(description)}\">\n<meta property=\"og:type\" content=\"website\">\n");
        sb.Append($"<meta property=\"og:image\" content=\"{BaseUrl}assets/banner.png\">\n<meta name=\"twitter:card\" content=\"summary_large_image\">\n");
        sb.Append("<link rel=\"icon\" type=\"image/png\" href=\"assets/icon.png\">\n<link rel=\"stylesheet\" href=\"assets/site.css\">\n<script src=\"assets/site.js\" defer></script>\n</head>\n<body>\n");
        sb.Append("<a class=\"skip\" href=\"#main\">Skip to the content</a>\n");
        sb.Append("<header class=\"top\"><a class=\"brand\" href=\"index.html\"><img src=\"assets/icon.png\" alt=\"\" width=\"28\" height=\"28\"><span>.Clarion</span></a>\n");
        sb.Append("<div class=\"search\"><label class=\"sr\" for=\"q\">Search every setting</label><input id=\"q\" type=\"search\" placeholder=\"Search every setting\" autocomplete=\"off\" spellcheck=\"false\"><div id=\"results\" role=\"listbox\" hidden></div></div>\n");
        sb.Append("<button id=\"theme\" class=\"quiet\" type=\"button\" aria-label=\"Switch between dark and light\">Theme</button></header>\n");
        sb.Append("<div class=\"shell\">\n<nav class=\"rail\" aria-label=\"Pages\"><ul>\n");
        foreach (var n in nav)
        {
            var current = n.File == file ? " aria-current=\"page\"" : "";
            var count = n.Count > 0 ? $"<span class=\"count\">{n.Count}</span>" : "";
            sb.Append($"<li><a href=\"{n.File}\"{current}><span>{H(n.Label)}</span>{count}</a></li>\n");
        }
        sb.Append($"</ul><ul class=\"outside\"><li><a href=\"{Repo}/releases/latest\">Download</a></li><li><a href=\"{Repo}\">Source on GitHub</a></li><li><a href=\"{Repo}/issues/new/choose\">Report a problem</a></li></ul></nav>\n");
        sb.Append($"<main id=\"main\" tabindex=\"-1\">\n{body}\n</main>\n</div>\n");
        sb.Append($"<footer><p>.Clarion {H(version)}. Written from the same catalog the app runs on, from the main branch, so it can describe settings that are newer than the release you have. The source is public on GitHub and all rights are reserved.</p></footer>\n</body>\n</html>\n");
        return sb.ToString();
    }

    private static string HomeBody(SiteSource s, IReadOnlyList<Group> groups, Group repair, Group cleanup)
    {
        var apps = s.Tweaks.Count(t => t.Category == "Install");
        var settings = s.Tweaks.Count - apps;
        var sb = new StringBuilder();
        sb.Append("<section class=\"hero\"><img src=\"assets/icon.png\" alt=\"\" width=\"88\" height=\"88\"><div>\n<h1>.Clarion</h1>\n<p class=\"tagline\">See it. Set it. Done.</p>\n");
        sb.Append($"<p class=\"lead\">A native Windows app to remove bloat, cut tracking, clean up, repair and tune your PC. This site explains all {settings} settings in it, plus {apps} apps it can install, {repair.Entries.Count} repair jobs and {cleanup.Entries.Count} cleanup rows. Each one says what it does, what you gain, what can go wrong and what it changes, down to the registry value.</p>\n");
        sb.Append($"<p class=\"actions\"><a class=\"button primary\" href=\"{Repo}/releases/latest\">Download</a><a class=\"button\" href=\"{Repo}\">Source on GitHub</a><a class=\"button\" href=\"presets.html\">See the presets</a></p>\n</div></section>\n");

        sb.Append("<section><h2>Browse by group</h2>\n<ul class=\"cards\">\n");
        foreach (var g in groups.Concat([repair, cleanup]))
            sb.Append($"<li><a class=\"card\" href=\"{g.File}\"><strong>{H(g.Title)}</strong><span class=\"count\">{g.Entries.Count}</span><span class=\"blurb\">{H(g.Blurb)}</span></a></li>\n");
        sb.Append("</ul></section>\n");

        sb.Append("<section><h2>How to read an entry</h2>\n<div class=\"legend\">\n");
        sb.Append("<div><h3>Risk</h3><dl>");
        foreach (var r in Enum.GetValues<RiskLevel>()) sb.Append($"<dt><span class=\"chip {RiskKind(r)}\">{H(TweakTooltip.RiskText(r))}</span></dt><dd>{H(TweakTooltip.RiskHint(r))}</dd>");
        sb.Append("</dl></div>\n");
        sb.Append("<div><h3>Suggestion</h3><dl>");
        foreach (var r in Enum.GetValues<Recommendation>()) sb.Append($"<dt><span class=\"chip {RecKind(r)}\">{H(TweakTooltip.Label(r))}</span></dt><dd>{H(TweakTooltip.RecommendationHint(r))}</dd>");
        sb.Append("</dl><p class=\"note\">An entry with Medium or High risk is never marked as suggested.</p></div>\n");
        sb.Append("<div><h3>Evidence</h3><dl>");
        foreach (var e in Enum.GetValues<Evidence>()) sb.Append($"<dt><span class=\"chip neutral\">{H(TweakTooltip.Label(e))}</span></dt><dd>{H(EvidenceHint(e))}</dd>");
        sb.Append("</dl><p class=\"note\">In the app, entries with Medium or High risk, and entries not proven by measurement, stay hidden until Expert mode is on.</p></div>\n</div>\n");
        sb.Append("<p>Every entry also lists the exact changes it makes, the Windows build it was last checked on, and where the claim comes from. Reverting an entry restores what was there before it was applied, because the app records the real previous value first.</p></section>\n");
        return sb.ToString();
    }

    private static string PresetsBody(SiteSource s)
    {
        var byId = s.Tweaks.ToDictionary(t => t.Id, StringComparer.OrdinalIgnoreCase);
        var fileOf = s.Tweaks.GroupBy(t => t.Category).ToDictionary(g => g.Key, g => Slug(CategoryTitle(g.Key)) + ".html");
        var sb = new StringBuilder();
        sb.Append("<section class=\"intro\"><h1>Presets</h1><p>A preset queues a group of settings. Nothing changes until you review the list and press Apply. Settings that are already on are marked in the app, so queueing a preset never repeats work.</p></section>\n");
        foreach (var (key, title, text) in PresetInfo.All)
        {
            if (!s.Presets.TryGetValue(key, out var ids)) continue;
            sb.Append($"<section id=\"{key}\" class=\"preset\"><h2>{H(title)} <span class=\"count\">{ids.Count}</span></h2><p>{H(text)}</p>\n<ul class=\"members\">\n");
            foreach (var id in ids)
            {
                if (!byId.TryGetValue(id, out var t)) continue;
                sb.Append($"<li><a href=\"{fileOf[t.Category]}#{H(t.Id)}\">{H(t.Name)}</a> <span class=\"chip {RiskKind(t.RiskLevel)}\">{H(TweakTooltip.RiskText(t.RiskLevel))}</span></li>\n");
            }
            sb.Append("</ul></section>\n");
        }
        return sb.ToString();
    }

    private static string GroupBody(Group g)
    {
        var sb = new StringBuilder();
        sb.Append($"<section class=\"intro\"><h1>{H(g.Title)}</h1><p>{H(g.Blurb)}</p>\n");
        sb.Append("<form class=\"filters\" role=\"search\" aria-label=\"Filter this page\" onsubmit=\"return false\">");
        sb.Append("<label>Filter <input id=\"filter\" type=\"search\" placeholder=\"Name or words in the text\" autocomplete=\"off\"></label>");
        sb.Append("<label>Risk <select id=\"risk\"><option value=\"\">Any</option>");
        foreach (var r in Enum.GetValues<RiskLevel>()) sb.Append($"<option value=\"{RiskClass(r)}\">{H(TweakTooltip.RiskText(r))}</option>");
        sb.Append("</select></label><label>Suggestion <select id=\"rec\"><option value=\"\">Any</option>");
        foreach (var r in Enum.GetValues<Recommendation>()) sb.Append($"<option value=\"{RecClass(r)}\">{H(TweakTooltip.Label(r))}</option>");
        sb.Append("</select></label><p id=\"shown\" role=\"status\" aria-live=\"polite\"></p></form></section>\n");
        foreach (var topic in g.Entries.GroupBy(e => e.Topic))
        {
            sb.Append("<section class=\"topic\">");
            if (topic.Key.Length > 0) sb.Append($"<h2>{H(topic.Key)} <span class=\"count\">{topic.Count()}</span></h2>");
            sb.Append('\n');
            foreach (var e in topic) Article(sb, e);
            sb.Append("</section>\n");
        }
        return sb.ToString();
    }

    private static void Article(StringBuilder sb, Entry e)
    {
        var haystack = $"{e.Name} {e.Summary} {e.Id} {e.What} {e.Benefit} {e.Risk}".ToLowerInvariant();
        sb.Append($"<article class=\"item\" id=\"{H(e.Id)}\" data-risk=\"{RiskClass(e.RiskLevel)}\" data-rec=\"{RecClass(e.Recommendation)}\" data-text=\"{H(haystack)}\">\n");
        sb.Append($"<h3><a class=\"anchor\" href=\"#{H(e.Id)}\">{H(e.Name)}</a></h3>\n<p class=\"summary\">{H(e.Summary)}</p>\n");
        sb.Append($"<ul class=\"chips\"><li class=\"chip {RiskKind(e.RiskLevel)}\" title=\"{H(TweakTooltip.RiskHint(e.RiskLevel))}\">{H(TweakTooltip.RiskText(e.RiskLevel))}</li><li class=\"chip {RecKind(e.Recommendation)}\" title=\"{H(TweakTooltip.RecommendationHint(e.Recommendation))}\">{H(TweakTooltip.Label(e.Recommendation))}</li>");
        if (e.Evidence is not null) sb.Append($"<li class=\"chip neutral\">{H(e.Evidence)}</li>");
        sb.Append("</ul>\n");
        sb.Append($"<p class=\"advice\"><strong>Our advice.</strong> {H(e.Advice)}</p>\n");
        if (e.Facts.Count > 0)
        {
            sb.Append("<ul class=\"facts\">");
            foreach (var f in e.Facts) sb.Append($"<li>{H(f)}</li>");
            sb.Append("</ul>\n");
        }
        sb.Append("<dl class=\"detail\">");
        if (!string.IsNullOrWhiteSpace(e.What)) sb.Append($"<dt>What it does</dt><dd>{H(e.What)}</dd>");
        sb.Append($"<dt>Benefit</dt><dd>{H(e.Benefit)}</dd><dt>Risk</dt><dd>{H(e.Risk)}</dd></dl>\n");
        if (e.Notes.Count > 0)
        {
            sb.Append("<ul class=\"notes\">");
            foreach (var n in e.Notes) sb.Append($"<li>{H(n)}</li>");
            sb.Append("</ul>\n");
        }
        if (e.Changes.Count > 0)
        {
            sb.Append($"<details><summary>{H(e.ChangesTitle)} ({e.Changes.Count})</summary><ul class=\"ops\">");
            foreach (var c in e.Changes) sb.Append($"<li><code>{H(c)}</code></li>");
            sb.Append("</ul>");
            if (e.Undo.Count > 0)
            {
                sb.Append("<p class=\"label\">Explicit undo, used when the previous value cannot be read back</p><ul class=\"ops\">");
                foreach (var c in e.Undo) sb.Append($"<li><code>{H(c)}</code></li>");
                sb.Append("</ul>");
            }
            sb.Append("</details>\n");
        }
        sb.Append($"<p class=\"meta\"><code>{H(e.Id)}</code>");
        foreach (var src in e.Sources.Where(IsHttp)) sb.Append($" · <a href=\"{H(src)}\" rel=\"noopener\">Source</a>");
        sb.Append("</p>\n");
        if (e.Provenance.Count > 0) sb.Append($"<p class=\"meta\">Built from the Microsoft policy definitions: {H(string.Join(", ", e.Provenance))}</p>\n");
        sb.Append("</article>\n");
    }

    private static string SearchIndex(IEnumerable<Group> pages)
    {
        var rows = pages.SelectMany(g => g.Entries.Select(e => new { n = e.Name, s = e.Summary, i = e.Id, g = g.Title, u = $"{g.File}#{e.Id}" }));
        return JsonSerializer.Serialize(rows);
    }

    // ---- Small helpers -------------------------------------------------------------------------------------------------------

    private static string H(string text) => WebUtility.HtmlEncode(text);

    private static bool IsHttp(string url) => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase);

    private static string RiskClass(RiskLevel r) => "risk-" + r.ToString().ToLowerInvariant();

    private static string RecClass(Recommendation r) => "rec-" + r.ToString().ToLowerInvariant();

    /// <summary>The chip colors the window uses: Safe and Low are green, Medium amber, High red.</summary>
    private static string RiskKind(RiskLevel r) => r switch { RiskLevel.Safe or RiskLevel.Low => "ok", RiskLevel.Medium => "warn", _ => "danger" };

    private static string RecKind(Recommendation r) => r switch { Recommendation.Recommended => "ok", Recommendation.OnlyIf => "warn", Recommendation.Avoid => "danger", _ => "neutral" };

    private static string EvidenceHint(Evidence e) => e switch
    {
        Evidence.Proven => "The effect is documented by the vendor and can be measured or seen.",
        Evidence.Situational => "Helps on some hardware or workloads. The entry says which.",
        Evidence.Cosmetic => "Changes how something looks or behaves, with no performance claim.",
        _ => "A folk tweak with no reliable measurement behind it.",
    };

    internal static string Slug(string text)
    {
        var sb = new StringBuilder();
        foreach (var c in text.ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(c)) sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }

    private static string JoinNames(IReadOnlyList<string> names) => names.Count switch
    {
        0 => "",
        1 => names[0],
        _ => string.Join(", ", names.Take(names.Count - 1)) + " and " + names[^1],
    };

    private static string Resource(string name)
    {
        var asm = typeof(CatalogSite).Assembly;
        using var stream = asm.GetManifestResourceStream(asm.GetManifestResourceNames().Single(n => n.EndsWith(".Reports.Site." + name, StringComparison.Ordinal)))!;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
