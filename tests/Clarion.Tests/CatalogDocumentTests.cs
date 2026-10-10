using Clarion.Core.Catalog;
using Clarion.Core.Profiles;
using Clarion.Core.Reports;

namespace Clarion.Tests;

public sealed class CatalogDocumentTests
{
    private static string RepoFile(params string[] parts)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) dir = dir.Parent;
        Assert.NotNull(dir);
        return Path.Combine([dir!.FullName, .. parts]);
    }

    [Fact]
    public void Every_setting_appears_with_its_name_risk_changes_and_sources()
    {
        var text = CatalogDocument.Markdown(CatalogLoader.LoadEmbedded());

        foreach (var t in CatalogLoader.LoadEmbedded())
        {
            Assert.Contains($"`{t.Id}`", text);
            Assert.Contains(t.Name, text);
            foreach (var s in t.Sources) Assert.Contains($"<{s}>", text);
        }
        Assert.Contains("## Privacy", text);
        Assert.Contains("**Changes:**", text);
    }

    [Fact]
    public void The_same_catalog_always_gives_the_same_page()
    {
        var tweaks = CatalogLoader.LoadEmbedded();
        Assert.Equal(CatalogDocument.Markdown(tweaks), CatalogDocument.Markdown(tweaks.Reverse().ToList()));
    }

    [Fact]
    public void The_command_is_listed_in_the_help_and_parsed()
    {
        Assert.Equal(CliMode.CatalogDoc, CliOptions.Parse(["--catalog-doc"]).Mode);
        Assert.Contains("--catalog-doc", CliOptions.HelpText);
    }

    [Fact]
    public void The_page_in_the_docs_folder_matches_the_catalog()
    {
        var path = RepoFile("docs", "CATALOG.md");
        Assert.True(File.Exists(path), "docs/CATALOG.md is missing. Write it with: Clarion.exe --catalog-doc > docs/CATALOG.md");

        var onDisk = File.ReadAllText(path).Replace("\r\n", "\n");
        var expected = CatalogDocument.Markdown(CatalogLoader.LoadEmbedded());

        Assert.True(onDisk == expected, "docs/CATALOG.md is out of date. Write it again with: Clarion.exe --catalog-doc > docs/CATALOG.md (the text is UTF-8).");
    }
}
