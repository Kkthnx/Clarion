using System.Security.Cryptography;
using Clarion.Core.Apps;
using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

/// <summary>
/// Apps without a Simple Icons outline show a picture saved inside the program. These tests hold every one of those files to the rules that keep
/// the Apps page honest and quick: it is where the list says it came from, it is a real picture of a sensible size, and nothing is left over.
/// </summary>
public sealed class AppLogoFilesTests
{
    private static string Folder()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine(dir!.FullName, "src", "Clarion.App", "Assets", AppLogoFiles.FolderName);
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return dir!.FullName;
    }

    private static IReadOnlyList<Tweak> Apps() =>
        CatalogLoader.LoadEmbedded().Where(t => t.Apply.OfType<InstallWingetPackage>().Any()).ToList();

    /// <summary>The width and height a PNG file states, read from its header without a picture library.</summary>
    private static (int Width, int Height) PngSize(byte[] bytes)
    {
        byte[] signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        Assert.True(bytes.Length > 33 && bytes.AsSpan(0, 8).SequenceEqual(signature), "not a PNG file");
        int Be(int at) => (bytes[at] << 24) | (bytes[at + 1] << 16) | (bytes[at + 2] << 8) | bytes[at + 3];
        return (Be(16), Be(20));
    }

    [Fact]
    public void Every_listed_logo_is_a_real_png_of_a_sensible_size_that_matches_its_checksum()
    {
        foreach (var (id, entry) in AppLogoFiles.Default.All)
        {
            var path = Path.Combine(Folder(), id + ".png");
            Assert.True(File.Exists(path), $"{id}: file is missing");
            var bytes = File.ReadAllBytes(path);
            var (w, h) = PngSize(bytes);
            Assert.True(w == h, $"{id}: {w}x{h} is not square");
            Assert.InRange(w, 48, 128);
            Assert.Equal(entry.Width, w);
            Assert.True(bytes.Length <= 80_000, $"{id}: {bytes.Length} bytes is more than a logo needs");
            Assert.Equal(entry.Sha256, Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
        }
    }

    [Fact]
    public void Every_logo_says_where_it_came_from_and_how_it_was_prepared()
    {
        foreach (var (id, entry) in AppLogoFiles.Default.All)
        {
            Assert.StartsWith("https://", entry.Source, StringComparison.Ordinal);
            Assert.Contains(entry.Plate, new[] { "none", "white", "dark" });
            Assert.Matches(@"^\d+x\d+$", entry.Original);
            Assert.True(entry.Sha256.Length == 64, id);
        }
    }

    [Fact]
    public void No_file_is_left_without_an_entry_and_no_entry_without_a_known_app()
    {
        var ids = Apps().Select(a => a.Id).ToHashSet(StringComparer.Ordinal);
        var onDisk = Directory.GetFiles(Folder(), "*.png").Select(f => Path.GetFileNameWithoutExtension(f)).ToHashSet(StringComparer.Ordinal);
        Assert.Empty(onDisk.Except(AppLogoFiles.Default.All.Keys));
        Assert.All(AppLogoFiles.Default.All.Keys, id => Assert.Contains(id, ids));
    }

    [Fact]
    public void An_app_has_an_outline_or_a_picture_never_both()
    {
        var outlines = AppIcons.Default.All.Keys.ToHashSet(StringComparer.Ordinal);
        Assert.All(AppLogoFiles.Default.All.Keys, id => Assert.DoesNotContain(id, outlines));
    }

    [Fact]
    public void A_logo_that_was_looked_at_and_rejected_is_not_in_the_program_and_says_why()
    {
        var scripts = Path.Combine(RepoRoot(), "scripts");
        using var doc = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(scripts, "app-logo-rejected.json")));
        var rejected = doc.RootElement.EnumerateObject().Where(p => !p.Name.StartsWith('_')).ToList();
        Assert.NotEmpty(rejected);
        var ids = Apps().Select(a => a.Id).ToHashSet(StringComparer.Ordinal);
        foreach (var p in rejected)
        {
            Assert.Contains(p.Name, ids);
            Assert.False(string.IsNullOrWhiteSpace(p.Value.GetString()), $"{p.Name} has no reason");
            Assert.False(File.Exists(Path.Combine(Folder(), p.Name + ".png")), $"{p.Name} is rejected but its file is still there");
            Assert.False(AppLogoFiles.Default.All.ContainsKey(p.Name), $"{p.Name} is rejected but still listed");
        }
    }

    [Fact]
    public void An_app_named_in_the_source_map_is_not_also_rejected()
    {
        var scripts = Path.Combine(RepoRoot(), "scripts");
        using var sources = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(scripts, "app-logo-sources.json")));
        using var rejected = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(scripts, "app-logo-rejected.json")));
        var rejectedIds = rejected.RootElement.EnumerateObject().Select(p => p.Name).ToHashSet(StringComparer.Ordinal);
        foreach (var p in sources.RootElement.EnumerateObject().Where(p => !p.Name.StartsWith('_')))
        {
            Assert.DoesNotContain(p.Name, rejectedIds);
            Assert.Contains(p.Value.GetProperty("collection").GetString(), new[] { "dashboard-icons", "selfhst" });
        }
    }

    [Fact]
    public void The_path_for_an_app_is_found_only_when_the_file_is_there()
    {
        var any = AppLogoFiles.Default.All.Keys.First();
        var assets = Path.GetDirectoryName(Folder())!;
        Assert.NotNull(AppLogoFiles.Default.PathFor(any, assets));
        Assert.Null(AppLogoFiles.Default.PathFor(any, Path.Combine(Path.GetTempPath(), "clarion-no-such-assets")));
        Assert.Null(AppLogoFiles.Default.PathFor("app.not-in-the-list", assets));
    }
}
