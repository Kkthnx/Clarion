using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

/// <summary>The readme states how many settings and apps there are, and a stale number is the first thing a reader would notice.</summary>
public sealed class ReadmeTests
{
    private static string Readme()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return File.ReadAllText(Path.Combine(dir!.FullName, "README.md"));
    }

    private static (int Settings, int Apps) Counts()
    {
        var all = CatalogLoader.LoadEmbedded();
        var apps = all.Count(t => t.Apply.OfType<InstallWingetPackage>().Any());
        return (all.Count - apps, apps);
    }

    [Fact]
    public void The_readme_gives_the_real_number_of_settings()
    {
        var (settings, _) = Counts();
        Assert.Contains($"| {settings} settings across", Readme(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_readme_gives_the_real_number_of_apps()
    {
        var (_, apps) = Counts();
        Assert.Contains($"| {apps} programs installed with winget", Readme(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_readme_does_not_promise_more_than_the_apps_page_delivers()
    {
        // Apps start winget, which makes network requests of its own, so the privacy lines must say so.
        var text = Readme();
        Assert.Contains("The Apps page runs winget", text, StringComparison.Ordinal);
        Assert.DoesNotContain("The only request Clarion can make is an update check", text, StringComparison.Ordinal);
        Assert.DoesNotContain("sends nothing anywhere. The only network request", text, StringComparison.Ordinal);
    }
}
