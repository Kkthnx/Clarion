using Clarion.Core.Updates;

namespace Clarion.Tests;

public sealed class ChangelogTests
{
    private const string Sample = """
        # Changelog

        ## Unreleased

        New
        - Not released yet.

        ## 0.1.0-beta.4

        New
        - Verify page, with `--verify` on the command line.
        - What broke page.

        Safer
        - History records the account.

        Fixed
        - A leak.
        - A sort bug.

        Polish
        - Selected cards stay highlighted.

        ## 0.1.0-beta.3

        - Fixed the build checks on GitHub.
        - New README.

        ## 0.1.0-beta.2

        - Installer.
        """;

    // ---- groups ----

    [Fact]
    public void Changes_are_grouped_under_the_heading_they_were_written_below()
    {
        var beta4 = ChangelogParser.ParseChangelog(Sample).Single(n => n.Version == "0.1.0-beta.4");

        Assert.Equal([ChangeKind.New, ChangeKind.Safer, ChangeKind.Fixed, ChangeKind.Polish], beta4.Groups.Select(g => g.Kind));
        Assert.Equal(2, beta4.Count(ChangeKind.New));
        Assert.Equal(2, beta4.Count(ChangeKind.Fixed));
        Assert.Equal(6, beta4.Total);
    }

    [Fact]
    public void Groups_come_out_in_a_fixed_order_whatever_order_they_were_written_in()
    {
        var groups = ChangelogParser.ParseGroups("Polish\n- p\n\nNew\n- n\n\nFixed\n- f");

        Assert.Equal([ChangeKind.New, ChangeKind.Fixed, ChangeKind.Polish], groups.Select(g => g.Kind));
    }

    [Fact]
    public void Backticks_are_not_shown_and_the_text_is_otherwise_kept()
    {
        var item = ChangelogParser.ParseGroups("New\n- Verify page, with `--verify` on the command line.").Single().Items.Single();

        Assert.Equal("Verify page, with --verify on the command line.", item);
    }

    [Fact]
    public void An_older_section_with_only_dash_lines_becomes_one_changes_group()
    {
        var beta3 = ChangelogParser.ParseChangelog(Sample).Single(n => n.Version == "0.1.0-beta.3");

        var group = Assert.Single(beta3.Groups);
        Assert.Equal(ChangeKind.Other, group.Kind);
        Assert.Equal("Changes", group.Title);
        Assert.Equal(2, group.Items.Count);
    }

    [Fact]
    public void Unreleased_is_not_a_release()
    {
        var all = ChangelogParser.ParseChangelog(Sample);

        Assert.DoesNotContain(all, n => n.Version.Equals("Unreleased", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(["0.1.0-beta.4", "0.1.0-beta.3", "0.1.0-beta.2"], all.Select(n => n.Version));
        Assert.All(all, n => Assert.True(n.PreRelease));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("just some text\nwith no list")]
    public void Text_without_changes_gives_no_groups_and_no_error(string? text) => Assert.Empty(ChangelogParser.ParseGroups(text));

    [Fact]
    public void Windows_line_endings_read_the_same()
    {
        var unix = ChangelogParser.ParseChangelog(Sample);
        var windows = ChangelogParser.ParseChangelog(Sample.Replace("\n", "\r\n"));

        Assert.Equal(unix.Select(n => n.Total), windows.Select(n => n.Total));
    }

    // ---- what the page shows ----

    private static IReadOnlyList<ReleaseNotes> Many(params string[] versions) =>
        versions.Select(v => new ReleaseNotes(v, true, null, null, [new ChangeGroup(ChangeKind.New, ["x"])])).ToList();

    [Fact]
    public void The_installed_release_and_the_ones_before_it_are_shown_newest_first_and_newer_ones_are_not()
    {
        var shown = WhatsNew.InstalledAndOlder("0.1.0-beta.3", Many("0.1.0-beta.5", "0.1.0-beta.4", "0.1.0-beta.3", "0.1.0-beta.2", "0.1.0-beta.1"), 5, out var left);

        Assert.Equal(["0.1.0-beta.3", "0.1.0-beta.2", "0.1.0-beta.1"], shown.Select(n => n.Version));
        Assert.Equal(0, left);
    }

    [Fact]
    public void Only_the_newest_few_are_shown_and_the_rest_are_counted()
    {
        var shown = WhatsNew.InstalledAndOlder("0.1.0-beta.9", Many("0.1.0-beta.9", "0.1.0-beta.8", "0.1.0-beta.7", "0.1.0-beta.6", "0.1.0-beta.5", "0.1.0-beta.4", "0.1.0-beta.3"), 5, out var left);

        Assert.Equal(5, shown.Count);
        Assert.Equal("0.1.0-beta.9", shown[0].Version);
        Assert.Equal(2, left);
    }

    [Fact]
    public void Beta_10_is_newer_than_beta_9_when_picking_the_history()
    {
        var shown = WhatsNew.InstalledAndOlder("0.1.0-beta.10", Many("0.1.0-beta.9", "0.1.0-beta.10"), 5, out _);

        Assert.Equal(["0.1.0-beta.10", "0.1.0-beta.9"], shown.Select(n => n.Version));
    }

    [Fact]
    public void A_version_that_cannot_be_read_shows_the_whole_history()
    {
        Assert.Equal(2, WhatsNew.InstalledAndOlder("dev", Many("0.1.0-beta.2", "0.1.0-beta.1"), 5, out _).Count);
    }

    [Fact]
    public void Totals_count_each_kind_across_releases_and_leave_out_kinds_with_nothing()
    {
        var notes = ChangelogParser.ParseChangelog(Sample).Where(n => n.Version != "0.1.0-beta.3" && n.Version != "0.1.0-beta.2");

        var totals = WhatsNew.Totals(notes);

        Assert.Equal(2, totals[ChangeKind.New]);
        Assert.Equal(1, totals[ChangeKind.Safer]);
        Assert.Equal(2, totals[ChangeKind.Fixed]);
        Assert.Equal(1, totals[ChangeKind.Polish]);
        Assert.False(totals.ContainsKey(ChangeKind.Other));
    }

    // ---- newer releases from GitHub ----

    private static ReleaseInfo Rel(string tag, string body = "", bool pre = true, bool draft = false) =>
        new(tag, "Clarion " + tag, $"https://github.com/Kkthnx/Clarion/releases/tag/{tag}", pre, draft, null, body);

    [Fact]
    public void Every_newer_release_is_listed_newest_first_so_a_skipped_version_is_not_missed()
    {
        var newer = UpdateCheck.NewerThan("0.1.0-beta.4", [Rel("v0.1.0-beta.4"), Rel("v0.1.0-beta.5"), Rel("v0.1.0-beta.6"), Rel("v0.1.0-beta.3")]);

        Assert.Equal(["v0.1.0-beta.6", "v0.1.0-beta.5"], newer.Select(r => r.Tag));
    }

    [Fact]
    public void Drafts_and_other_projects_pages_are_left_out_of_the_newer_list()
    {
        var newer = UpdateCheck.NewerThan("0.1.0-beta.4", [Rel("v0.1.0-beta.5", draft: true),
            new ReleaseInfo("v0.1.0-beta.6", "n", "https://evil.example/x", true, false, null)]);

        Assert.Empty(newer);
    }

    [Fact]
    public void The_notes_of_a_newer_release_come_from_its_body_in_the_same_format()
    {
        var body = "New\n- Thing one.\n- Thing two.\n\nFixed\n- A fix.";

        var notes = WhatsNew.FromReleases([Rel("v0.1.0-beta.5", body)]).Single();

        Assert.Equal("0.1.0-beta.5", notes.Version);
        Assert.Equal(2, notes.Count(ChangeKind.New));
        Assert.Equal(1, notes.Count(ChangeKind.Fixed));
        Assert.NotNull(notes.Url);
    }

    [Fact]
    public void A_release_with_no_notes_still_appears_with_no_groups()
    {
        var notes = WhatsNew.FromReleases([Rel("v0.1.0-beta.5", "")]).Single();

        Assert.Empty(notes.Groups);
        Assert.Equal(0, notes.Total);
    }

    [Fact]
    public void A_release_body_is_read_from_the_json_github_sends()
    {
        var json = "[{\"tag_name\":\"v0.1.0-beta.5\",\"name\":\"n\",\"html_url\":\"https://github.com/Kkthnx/Clarion/releases/tag/v0.1.0-beta.5\"," +
                   "\"draft\":false,\"prerelease\":true,\"body\":\"New\\r\\n- One thing.\"}]";

        var release = UpdateCheck.ParseReleases(json).Single();

        Assert.Equal(1, WhatsNew.FromReleases([release]).Single().Total);
    }

    // ---- the real changelog, so a careless edit cannot break the page ----

    [Fact]
    public void The_changelog_inside_the_app_has_a_release_for_the_version_being_built()
    {
        var all = ChangelogParser.LoadEmbedded();
        var building = System.Reflection.CustomAttributeExtensions
            .GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>(typeof(ChangelogParser).Assembly)!.InformationalVersion.Split('+')[0];

        Assert.NotEmpty(all);
        Assert.Contains(all, n => n.Version == building);
    }

    [Fact]
    public void Every_release_in_the_real_changelog_has_at_least_one_change()
    {
        foreach (var n in ChangelogParser.LoadEmbedded())
            Assert.True(n.Total > 0, $"{n.Version} has no changes the page can show.");
    }

    [Fact]
    public void Every_heading_in_the_real_changelog_is_one_the_page_knows()
    {
        var known = new[] { "New", "Safer", "Fixed", "Polish" };
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "CHANGELOG.md");
        if (!File.Exists(path)) return;

        // A bare line of one or two words that is not a dash line is a heading. Anything but the four known ones would be silently dropped.
        var unknown = File.ReadAllLines(path)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0 && !l.StartsWith("-") && !l.StartsWith("#") && l.Split(' ').Length <= 2 && !l.Contains('.'))
            .Where(l => !known.Contains(l, StringComparer.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(unknown);
    }

    [Fact]
    public void The_real_changelog_lists_releases_newest_first_so_the_history_reads_in_order()
    {
        var versions = ChangelogParser.LoadEmbedded().Select(n => ReleaseVersion.Parse(n.Version)!).ToList();

        Assert.Equal(versions.OrderByDescending(v => v, Comparer<ReleaseVersion>.Create((a, b) => a.CompareTo(b))), versions);
    }
}
