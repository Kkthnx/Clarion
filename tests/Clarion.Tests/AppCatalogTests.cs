using Clarion.Core.Apps;
using Clarion.Core.Catalog;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class AppCatalogTests
{
    private static IReadOnlyList<Tweak> Apps() =>
        CatalogLoader.LoadEmbedded().Where(t => t.Apply.OfType<InstallWingetPackage>().Any()).ToList();

    [Fact]
    public void Battle_net_carries_the_folder_its_manifest_requires()
    {
        var battle = Apps().Single(t => t.Id == "app.battle-net");
        var install = Assert.IsType<InstallWingetPackage>(Assert.Single(battle.Apply));
        Assert.Equal(@"%ProgramFiles(x86)%\Battle.net", install.Location);
    }

    [Fact]
    public void Every_app_has_a_unique_id_and_a_unique_package()
    {
        var apps = Apps();
        Assert.Equal(apps.Count, apps.Select(a => a.Id).Distinct(StringComparer.Ordinal).Count());
        var packages = apps.Select(a => ((InstallWingetPackage)a.Apply[0]).Id).ToList();
        Assert.Equal(packages.Count, packages.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Every_app_undoes_with_the_uninstall_of_the_same_package()
    {
        foreach (var app in Apps())
        {
            var install = (InstallWingetPackage)Assert.Single(app.Apply);
            var undo = Assert.IsType<UninstallWingetPackage>(Assert.Single(app.Undo));
            Assert.Equal(install.Id, undo.Id);
        }
    }

    [Fact]
    public void Every_app_names_where_the_package_is_defined()
    {
        foreach (var app in Apps())
            Assert.Contains(app.Sources, s => s.Contains("winget-pkgs", StringComparison.Ordinal));
    }

    [Fact]
    public void Apps_that_forbid_an_administrator_install_are_left_out()
    {
        // Spotify's winget manifest sets ElevationRequirement: elevationProhibited, and Clarion always installs as administrator.
        Assert.DoesNotContain(Apps(), a => ((InstallWingetPackage)a.Apply[0]).Id.Equals("Spotify.Spotify", StringComparison.OrdinalIgnoreCase));
    }
}
