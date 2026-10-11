using Clarion.Core.Apps;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class WingetLocationTests
{
    private static string? Vars(string name) => name switch
    {
        "ProgramFiles(x86)" => @"C:\Program Files (x86)",
        "ProgramFiles" => @"C:\Program Files",
        "LocalAppData" => @"C:\Users\Jane Doe\AppData\Local",
        _ => null,
    };

    [Theory]
    [InlineData(@"%ProgramFiles(x86)%\Battle.net")]
    [InlineData(@"%ProgramFiles%\Some App")]
    [InlineData(@"%LocalAppData%\Programs\Tool")]
    public void Folders_under_the_allowed_roots_are_accepted(string location) => Assert.True(WingetLocations.IsValid(location));

    [Theory]
    [InlineData(@"C:\Program Files\App")]
    [InlineData(@"%ProgramFiles%")]
    [InlineData(@"%ProgramFiles%\..\Windows")]
    [InlineData(@"%ProgramFiles%\App"" --force")]
    [InlineData(@"%SystemRoot%\System32")]
    [InlineData(@"%ProgramFiles%\App\")]
    [InlineData(@"%ProgramFiles%\\App")]
    [InlineData("")]
    public void Anything_else_is_refused(string location) => Assert.False(WingetLocations.IsValid(location));

    [Fact]
    public void The_variable_is_filled_in_for_this_pc()
    {
        Assert.Equal(@"C:\Program Files (x86)\Battle.net", WingetLocations.Expand(@"%ProgramFiles(x86)%\Battle.net", Vars));
        Assert.Equal(@"C:\Users\Jane Doe\AppData\Local\Programs\Tool", WingetLocations.Expand(@"%LocalAppData%\Programs\Tool", Vars));
    }

    [Fact]
    public void An_unset_variable_is_an_error_not_an_empty_path() =>
        Assert.Throws<ArgumentException>(() => WingetLocations.Expand(@"%ProgramFiles%\App", _ => null));

    [Fact]
    public void The_install_command_names_the_folder_only_when_one_is_given()
    {
        Assert.DoesNotContain("--location", WingetCommands.Install("7zip.7zip"), StringComparison.Ordinal);
        var withFolder = WingetCommands.Install("Blizzard.BattleNet", @"%ProgramFiles%\Some App");
        Assert.Contains("--location \"", withFolder, StringComparison.Ordinal);
        Assert.EndsWith("Some App\"", withFolder, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => WingetCommands.Install("Blizzard.BattleNet", @"C:\Windows"));
    }

    [Fact]
    public void The_handler_passes_the_folder_to_winget()
    {
        var store = new FakeWingetStore { AcceptPackageAgreements = true };
        new WingetHandler(store).Execute(new InstallWingetPackage("Blizzard.BattleNet", @"%ProgramFiles(x86)%\Battle.net"));
        Assert.Equal(@"install Blizzard.BattleNet at %ProgramFiles(x86)%\Battle.net", Assert.Single(store.Calls));
    }

    [Fact]
    public void A_bad_folder_in_the_catalog_is_caught_when_it_loads()
    {
        var tweak = CatalogLoader.LoadEmbedded().Single(t => t.Id == "app.battle-net") with
        {
            Apply = [new InstallWingetPackage("Blizzard.BattleNet", @"C:\Windows")],
        };
        Assert.Contains(CatalogLoader.Validate([tweak]), e => e.Contains("install folder", StringComparison.Ordinal));
    }

    [Fact]
    public void The_install_location_code_has_a_plain_sentence() =>
        Assert.Contains("install folder", WingetCodes.Explain(unchecked((int)0x8A15005F)), StringComparison.Ordinal);
}
