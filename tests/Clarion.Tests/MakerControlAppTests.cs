using Clarion.Core.Startup;

namespace Clarion.Tests;

public sealed class MakerControlAppTests
{
    private static StartupEntry Built(string name, string command)
    {
        var source = new FakeStartupSource();
        source.Raw.Add(new RawStartupItem(StartupOrigin.MachineRun, name, command));
        return StartupList.Build(source).Single();
    }

    [Theory]
    [InlineData("ArmouryCrate.UserSessionHelper", "\"C:\\Program Files (x86)\\ASUS\\ArmouryDevice\\ArmouryCrate.UserSessionHelper.exe\"", "Armoury Crate")]
    [InlineData("Lenovo Vantage", "\"C:\\Program Files\\Lenovo\\Lenovo Vantage\\LenovoVantage.exe\"", "Lenovo Vantage and Legion Zone")]
    [InlineData("LegionZone", "\"C:\\Program Files\\Lenovo\\LegionZone\\LegionZone.exe\"", "Lenovo Vantage and Legion Zone")]
    [InlineData("MSI Center", "\"C:\\Program Files (x86)\\MSI\\MSI Center\\MSI Center.exe\"", "MSI Center")]
    [InlineData("AlienwareCommandCenter", "\"C:\\Program Files\\Alienware\\Alienware Command Center\\AWCC.exe\"", "Alienware Command Center")]
    [InlineData("HP System Event Utility", "\"C:\\Program Files\\HP\\HP System Event Utility\\HPSystemEventUtility.exe\"", "HP System Event Utility")]
    [InlineData("NitroSense", "\"C:\\Program Files\\NitroSense\\NitroSense.exe\"", "NitroSense and PredatorSense")]
    public void The_makers_control_apps_are_recognised_and_explained(string name, string command, string product)
    {
        var entry = Built(name, command);

        Assert.Equal(product, MakerControlApps.Find(entry)?.Product);
        Assert.Contains(product, entry.Warning, StringComparison.Ordinal);
        Assert.Contains("turn it back on", entry.Warning, StringComparison.Ordinal);
        // The entry can still be switched off, since the warning only informs.
        Assert.False(entry.IsProtected);
    }

    [Theory]
    [InlineData("Spotify", "\"C:\\Users\\a\\AppData\\Roaming\\Spotify\\Spotify.exe\" --autostart")]
    [InlineData("Discord", "\"C:\\Users\\a\\AppData\\Local\\Discord\\Discord.exe\"")]
    [InlineData("RtkAudUService", "\"C:\\Windows\\System32\\DriverStore\\FileRepository\\realtekservice.inf_amd64\\RtkAudUService64.exe\" -background")]
    [InlineData("Lenovo Welcome", "\"C:\\Program Files\\Lenovo\\Welcome\\welcome.exe\"")]
    [InlineData("HP Support Assistant", "\"C:\\Program Files\\HP\\HP Support Framework\\HPSF.exe\"")]
    public void Other_programs_and_a_makers_promotions_get_no_control_app_warning(string name, string command) =>
        Assert.Null(Built(name, command).Warning);

    [Fact]
    public void Windows_Security_keeps_its_own_warning()
    {
        var entry = Built("SecurityHealth", "C:\\WINDOWS\\system32\\SecurityHealthSystray.exe");
        Assert.Contains("Windows Security", entry.Warning, StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_program_says_it_is_gone_before_it_says_anything_else()
    {
        var source = new FakeStartupSource();
        source.Missing.Add("C:\\gone\\ArmouryCrate.exe");
        source.Raw.Add(new RawStartupItem(StartupOrigin.UserRun, "ArmouryCrate", "\"C:\\gone\\ArmouryCrate.exe\""));

        Assert.Contains("not there any more", StartupList.Build(source).Single().Warning, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_app_names_its_source_and_says_what_it_handles()
    {
        foreach (var app in MakerControlApps.All)
        {
            Assert.StartsWith("https://", app.Source);
            Assert.True(app.Handles.Length >= 30, app.Product);
            Assert.NotEmpty(app.Names);
            Assert.DoesNotContain(';', MakerControlApps.Warning(app));
            Assert.DoesNotContain('\u2014', MakerControlApps.Warning(app));
        }
    }
}
