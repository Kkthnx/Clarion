using Clarion.Core.Catalog;
using Clarion.Core.Cleanup;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class BrowserCacheTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "clarion-browser-" + Guid.NewGuid().ToString("N"));
    private readonly FakeCleanupPlatform _p;
    private readonly CleanupEngine _engine;

    public BrowserCacheTests()
    {
        Directory.CreateDirectory(_root);
        _p = new FakeCleanupPlatform(_root);
        _engine = new CleanupEngine(_p);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }

    private string Make(string relative, int bytes)
    {
        var path = Path.Combine(_p.Local, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, new byte[bytes]);
        return path;
    }

    private static CleanTarget Chrome(params string[] processes) => new()
    {
        Id = "b",
        Group = "g",
        Name = "b",
        Summary = "s",
        Advice = "a",
        Benefit = "b",
        Risk = "r",
        RiskLevel = RiskLevel.Safe,
        Facts = ["one", "two"],
        Processes = processes,
        Rules = [new ProfileFoldersRule("%LOCALAPPDATA%\\Google\\Chrome\\User Data", ["Cache", "Code Cache", "GPUCache"])],
    };

    [Fact]
    public void Every_profile_is_cleaned_and_what_is_not_a_cache_is_kept()
    {
        var a = Make("Google\\Chrome\\User Data\\Default\\Cache\\Cache_Data\\f_1", 1000);
        var b = Make("Google\\Chrome\\User Data\\Default\\Code Cache\\js\\x", 200);
        var c = Make("Google\\Chrome\\User Data\\Profile 2\\GPUCache\\data_0", 30);
        var cookies = Make("Google\\Chrome\\User Data\\Default\\Network\\Cookies", 50);
        var logins = Make("Google\\Chrome\\User Data\\Profile 2\\Login Data", 60);
        var state = Make("Google\\Chrome\\User Data\\Local State", 70);

        var result = _engine.Clean(Chrome(), false, false, null, default);

        Assert.Equal(1230, result.BytesFreed);
        Assert.False(File.Exists(a));
        Assert.False(File.Exists(b));
        Assert.False(File.Exists(c));
        Assert.True(File.Exists(cookies));
        Assert.True(File.Exists(logins));
        Assert.True(File.Exists(state));
    }

    [Fact]
    public void The_scan_adds_up_the_same_folders()
    {
        Make("Google\\Chrome\\User Data\\Default\\Cache\\f", 400);
        Make("Google\\Chrome\\User Data\\Profile 1\\Cache\\f", 100);

        var scan = _engine.Scan(Chrome());

        Assert.Equal(500, scan.Bytes);
        Assert.Equal(2, scan.Files);
    }

    [Fact]
    public void A_missing_browser_is_simply_not_there()
    {
        var scan = _engine.Scan(Chrome());

        Assert.False(scan.Present);
        Assert.Equal(0, scan.Bytes);
    }

    [Fact]
    public void An_open_browser_is_left_alone()
    {
        var file = Make("Google\\Chrome\\User Data\\Default\\Cache\\f", 400);
        _p.Running.Add("chrome");

        var result = _engine.Clean(Chrome("chrome"), false, false, null, default);

        Assert.True(result.Skipped);
        Assert.True(File.Exists(file));
    }

    [Fact]
    public void A_preview_removes_nothing()
    {
        var file = Make("Google\\Chrome\\User Data\\Default\\Cache\\f", 400);

        var result = _engine.Clean(Chrome(), false, true, null, default);

        Assert.Equal(400, result.BytesFreed);
        Assert.True(File.Exists(file));
    }

    [Fact]
    public void The_catalog_rows_are_valid_and_each_browser_stops_when_it_runs()
    {
        var rows = CatalogLoader.LoadCleanup().Where(t => t.Group == "Browser caches").ToList();

        Assert.Equal(["browsers.brave", "browsers.chrome", "browsers.edge", "browsers.firefox"], rows.Select(r => r.Id).Order().ToList());
        Assert.All(rows, r => Assert.NotEmpty(r.Processes));
        Assert.All(rows, r => Assert.All(r.Rules, rule => Assert.IsType<ProfileFoldersRule>(rule)));
        Assert.Empty(CatalogLoader.ValidateCleanup(rows));
    }

    [Theory]
    [InlineData("..\\x")]
    [InlineData("a\\b")]
    [InlineData("")]
    public void A_folder_name_that_leaves_the_profile_is_rejected(string name)
    {
        var target = Chrome() with { Rules = [new ProfileFoldersRule("%LOCALAPPDATA%\\Google", [name])] };
        Assert.Contains(CatalogLoader.ValidateCleanup([target]), e => e.Contains("plain folder names", StringComparison.Ordinal));
    }
}
