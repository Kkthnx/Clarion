using System.Text.RegularExpressions;
using Clarion.Core.Apps;
using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class AppIconTests
{
    [Fact]
    public void EveryIconBelongsToAnApp()
    {
        var apps = CatalogLoader.LoadEmbedded().Where(t => t.Apply.OfType<InstallWingetPackage>().Any()).Select(t => t.Id).ToHashSet();
        foreach (var id in AppIcons.Default.All.Keys)
            Assert.Contains(id, apps);
    }

    [Fact]
    public void EveryIconHasAColourAPathAndAnOfficialSource()
    {
        Assert.NotEmpty(AppIcons.Default.All);
        foreach (var (id, icon) in AppIcons.Default.All)
        {
            Assert.Matches("^[0-9A-F]{6}$", icon.Hex);
            Assert.False(string.IsNullOrWhiteSpace(icon.Title), id);
            Assert.StartsWith("https://", icon.Source, StringComparison.Ordinal);
            // The path is drawn as given, so it may hold only path commands and numbers.
            Assert.Matches(new Regex(@"^[MmLlHhVvCcSsQqTtAaZz0-9eE.,\s\-+]+$"), icon.Path);
            Assert.True(icon.Path.Length is > 50 and < 8000, id);
        }
    }

    [Fact]
    public void TheSetIsNamedForTheCredits()
    {
        Assert.Contains("Simple Icons", AppIcons.Default.Set, StringComparison.Ordinal);
        Assert.StartsWith("https://", AppIcons.Default.SetUrl, StringComparison.Ordinal);
    }

    [Fact]
    public void ColoursThatWouldVanishAreFlagged()
    {
        var black = new AppIcon("x", "000000", "https://example.com", "M0 0");
        var white = new AppIcon("x", "FFFFFF", "https://example.com", "M0 0");
        var blue = new AppIcon("x", "2496ED", "https://example.com", "M0 0");
        Assert.False(black.ReadableOn(darkBackground: true));
        Assert.True(black.ReadableOn(darkBackground: false));
        Assert.True(white.ReadableOn(darkBackground: true));
        Assert.False(white.ReadableOn(darkBackground: false));
        Assert.True(blue.ReadableOn(darkBackground: true));
        Assert.True(blue.ReadableOn(darkBackground: false));
    }

    [Fact]
    public void RgbReadsTheHex()
    {
        var icon = new AppIcon("x", "FB542B", "https://example.com", "M0 0");
        Assert.Equal(((byte)0xFB, (byte)0x54, (byte)0x2B), icon.Rgb);
    }

    [Fact]
    public void AppsWithoutAnIconAreStillFine()
    {
        Assert.Null(AppIcons.Default.For("app.putty"));
        Assert.NotNull(AppIcons.Default.For("app.brave"));
    }
}
