using Clarion.Core.SystemInfo;

namespace Clarion.Tests;

public sealed class BatteryGuardTests
{
    [Theory]
    [InlineData(30, true)]
    [InlineData(12, true)]
    [InlineData(0, true)]
    [InlineData(31, false)]
    [InlineData(80, false)]
    public void A_low_battery_off_the_charger_is_warned_about_and_a_healthy_one_is_not(int percent, bool warned)
    {
        var warning = BatteryGuard.Warning(new PowerSource(true, OnBattery: true, Charging: false, percent), "Updating apps");

        Assert.Equal(warned, warning is not null);
    }

    [Fact]
    public void The_warning_names_the_job_and_the_charge_and_offers_both_ways_out()
    {
        var warning = BatteryGuard.Warning(new PowerSource(true, true, false, 22), "Updating apps");

        Assert.NotNull(warning);
        Assert.StartsWith("Updating apps", warning);
        Assert.Contains("22%", warning);
        Assert.Contains("Plug it in first, or start anyway", warning);
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(4, false)]
    [InlineData(5, true)]
    [InlineData(20, true)]
    public void A_short_job_is_never_warned_about_and_a_long_one_says_how_long(int minutes, bool warned)
    {
        var warning = BatteryGuard.Warning(new PowerSource(true, true, false, 15), "Scan the Windows image", minutes);

        Assert.Equal(warned, warning is not null);
        if (warned) Assert.Contains($"takes about {minutes} minutes", warning);
    }

    [Fact]
    public void A_low_battery_on_the_charger_or_taking_charge_is_not_warned_about()
    {
        Assert.Null(BatteryGuard.Warning(new PowerSource(true, OnBattery: false, Charging: false, 10), "x"));
        Assert.Null(BatteryGuard.Warning(new PowerSource(true, OnBattery: true, Charging: true, 10), "x"));
    }

    [Fact]
    public void A_desktop_and_a_charge_nobody_can_read_are_never_warned_about()
    {
        Assert.Null(BatteryGuard.Warning(PowerSource.None, "x"));
        Assert.Null(BatteryGuard.Warning(new PowerSource(true, true, false, null), "x"));
        Assert.Null(BatteryGuard.Warning(new PowerSource(false, true, false, 5), "x"));
    }
}
