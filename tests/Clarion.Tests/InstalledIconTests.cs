using System.Runtime.Versioning;
using Clarion.Core.Apps;
using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Engine;
using Microsoft.Win32;

namespace Clarion.Tests;

public sealed class InstalledIconTests
{
    private static string? Same(string s) => s;

    [Theory]
    [InlineData(@"C:\Program Files\7-Zip\7zFM.exe", @"C:\Program Files\7-Zip\7zFM.exe")]
    [InlineData(@"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe,0", @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe")]
    [InlineData("\"C:\\Program Files\\App\\app.exe\",2", @"C:\Program Files\App\app.exe")]
    [InlineData("\"C:\\Program Files\\App\\app.exe\"", @"C:\Program Files\App\app.exe")]
    [InlineData(@"C:\Program Files\App\app.ico", @"C:\Program Files\App\app.ico")]
    [InlineData(@"  C:\Program Files\App\app.exe , -3 ", @"C:\Program Files\App\app.exe")]
    public void A_display_icon_names_its_file(string value, string expected) =>
        Assert.Equal(expected, DisplayIcons.ResolveFile(value, s => s));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(@"\\server\share\app.exe")]
    [InlineData(@"\\?\C:\app.exe")]
    [InlineData(@"app.exe")]
    [InlineData(@"C:app.exe")]
    [InlineData(@"C:\Program Files\App\..\..\Windows\evil.exe")]
    [InlineData(@"C:\Program Files\App\app.dll")]
    [InlineData(@"C:\Program Files\App\readme.txt")]
    [InlineData("\"C:\\Program Files\\App\\app.exe")]
    public void Anything_that_is_not_a_local_exe_or_ico_file_is_refused(string? value) =>
        Assert.Null(DisplayIcons.ResolveFile(value, s => s));

    [Fact]
    public void Environment_variables_are_filled_in()
    {
        var file = DisplayIcons.ResolveFile(@"%ProgramFiles%\App\app.exe", s => s.Replace("%ProgramFiles%", @"C:\Program Files", StringComparison.Ordinal));
        Assert.Equal(@"C:\Program Files\App\app.exe", file);
    }

    [Fact]
    public void Every_product_code_belongs_to_an_app_and_is_a_plain_registry_key_name()
    {
        var apps = CatalogLoader.LoadEmbedded().Where(t => t.Apply.OfType<InstallWingetPackage>().Any()).Select(t => t.Id).ToHashSet();
        Assert.NotEmpty(AppProducts.Default.All);
        foreach (var (id, codes) in AppProducts.Default.All)
        {
            Assert.Contains(id, apps);
            Assert.NotEmpty(codes);
            Assert.All(codes, c => Assert.True(c.Length is > 0 and <= 120 && !c.Contains('\\') && !c.Contains('/'), $"{id}: {c}"));
        }
    }

    [Fact]
    public void An_app_with_no_code_has_none()
    {
        Assert.Empty(AppProducts.Default.For("app.steam"));
        Assert.Contains("Battle.net", AppProducts.Default.For("app.battle-net"));
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void The_icon_file_is_found_through_the_uninstall_key()
    {
        var code = "ClarionTest-" + Guid.NewGuid().ToString("N");
        var file = Path.Combine(Path.GetTempPath(), code + ".exe");
        File.WriteAllBytes(file, [0x4D, 0x5A]);
        var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + code;
        try
        {
            using (var key = Registry.CurrentUser.CreateSubKey(path))
                key.SetValue("DisplayIcon", "\"" + file + "\",0");

            Assert.Equal(file, WindowsInstalledIcons.FindIconFile(["Not-a-real-key", code], includeUserKeys: true));
            // The current user's keys can be written by any program, so an elevated Clarion does not trust them.
            Assert.Null(WindowsInstalledIcons.FindIconFile([code], includeUserKeys: false));
            Assert.Null(WindowsInstalledIcons.FindIconFile(["Not-a-real-key"]));
            Assert.Null(WindowsInstalledIcons.FindIconFile([@"..\" + code, code.Replace("-", "/", StringComparison.Ordinal)]));
        }
        finally
        {
            Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
            File.Delete(file);
        }
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public void A_missing_icon_file_is_not_returned()
    {
        var code = "ClarionTest-" + Guid.NewGuid().ToString("N");
        var path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\" + code;
        try
        {
            using (var key = Registry.CurrentUser.CreateSubKey(path))
                key.SetValue("DisplayIcon", @"C:\Definitely\Not\Here\" + code + ".exe");
            Assert.Null(WindowsInstalledIcons.FindIconFile([code], includeUserKeys: true));
        }
        finally
        {
            Registry.CurrentUser.DeleteSubKeyTree(path, throwOnMissingSubKey: false);
        }
    }
}
