using System.Text.Json;
using Clarion.Core.Apps;
using Clarion.Core.Catalog;

namespace Clarion.Tests;

public sealed class AppSetTests
{
    private static readonly IReadOnlyList<Clarion.Core.Model.Tweak> Catalog = CatalogLoader.LoadEmbedded();

    [Fact]
    public void A_saved_file_has_the_shape_winget_export_writes()
    {
        var json = WingetExport.Serialize(["Git.Git", "7zip.7zip"], new DateTimeOffset(2026, 10, 15, 23, 9, 57, TimeSpan.Zero));
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.Equal("https://aka.ms/winget-packages.schema.2.0.json", root.GetProperty("$schema").GetString());
        Assert.StartsWith("2026-10-15T23:09:57", root.GetProperty("CreationDate").GetString());
        var source = Assert.Single(root.GetProperty("Sources").EnumerateArray());
        Assert.Equal(["7zip.7zip", "Git.Git"], source.GetProperty("Packages").EnumerateArray().Select(p => p.GetProperty("PackageIdentifier").GetString()!).ToArray());
        var details = source.GetProperty("SourceDetails");
        Assert.Equal("winget", details.GetProperty("Name").GetString());
        Assert.Equal("Microsoft.Winget.Source_8wekyb3d8bbwe", details.GetProperty("Identifier").GetString());
        Assert.Equal("Microsoft.PreIndexed.Package", details.GetProperty("Type").GetString());
        Assert.Equal("https://cdn.winget.microsoft.com/cache", details.GetProperty("Argument").GetString());
    }

    [Fact]
    public void A_saved_file_reads_back_to_the_same_packages()
    {
        var json = WingetExport.Serialize(["Git.Git", "git.git", "Brave.Brave"], DateTimeOffset.UnixEpoch);
        Assert.Equal(["Brave.Brave", "Git.Git"], WingetExport.InstalledIds(json).Order(StringComparer.OrdinalIgnoreCase).ToArray());
    }

    [Fact]
    public void Packages_become_catalog_settings_and_unknown_ones_are_reported_not_installed()
    {
        var (matched, unknown) = AppSets.FromPackages(Catalog, ["git.git", "Brave.Brave", "Nobody.Nothing", "nobody.nothing"]);

        Assert.Equal(["app.git", "app.brave"], matched);
        Assert.Equal(["Nobody.Nothing"], unknown);
    }

    [Fact]
    public void Catalog_settings_become_package_ids_and_settings_that_are_not_apps_are_left_out()
    {
        var packages = AppSets.PackagesOf(Catalog, ["app.git", "app.brave", Catalog.First(t => !t.Id.StartsWith("app.", StringComparison.Ordinal)).Id]);
        Assert.Equal(["Brave.Brave", "Git.Git"], packages);
    }

    [Fact]
    public void Every_app_in_the_catalog_survives_a_save_and_a_load()
    {
        var apps = Catalog.Where(t => t.Id.StartsWith("app.", StringComparison.Ordinal)).Select(t => t.Id).ToList();
        var packages = AppSets.PackagesOf(Catalog, apps);
        var (matched, unknown) = AppSets.FromPackages(Catalog, WingetExport.InstalledIds(WingetExport.Serialize(packages, DateTimeOffset.UnixEpoch)));

        Assert.Empty(unknown);
        Assert.Equal(apps.Order(StringComparer.Ordinal), matched.Order(StringComparer.Ordinal));
    }
}
