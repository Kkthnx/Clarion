using Clarion.Core.Model;
using Clarion.Core.Profiles;

namespace Clarion.Tests;

public sealed class NotAvailableReasonTests
{
    private static Tweak Make(string id, Requirements? requires = null) => new()
    {
        Id = id,
        Category = "T",
        Name = id,
        Summary = "s",
        What = "w",
        Benefit = "b",
        Risk = "r",
        Evidence = Evidence.Cosmetic,
        RiskLevel = RiskLevel.Safe,
        Requires = requires ?? new Requirements(),
        Apply = [new SetRegistryValue(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", id), new RegistryData(RegistryKind.DWord, "1"))],
    };

    [Fact]
    public void A_setting_for_a_newer_windows_says_so()
    {
        var profile = new MachineProfile(19045, "Professional");
        Assert.Equal("Needs Windows 11", profile.WhyNotSupported(new Requirements { MinBuild = 22000 }));
        Assert.Equal("Needs Windows build 20000 or later", profile.WhyNotSupported(new Requirements { MinBuild = 20000 }));
    }

    [Fact]
    public void A_setting_for_other_editions_names_them()
    {
        var profile = new MachineProfile(26200, "Professional");
        var why = profile.WhyNotSupported(new Requirements { Editions = ["Enterprise", "Education"] });
        Assert.Equal("Not supported on Professional. Works on Enterprise, Education", why);
    }

    [Fact]
    public void A_supported_setting_has_no_reason() =>
        Assert.Null(new MachineProfile(26200, "Professional").WhyNotSupported(new Requirements()));

    [Fact]
    public void The_plan_keeps_a_reason_for_each_setting_it_cannot_turn_on()
    {
        var edition = Make("edition", new Requirements { Editions = ["Enterprise"] });
        var gone = Make("gone");
        var plan = CliPlanner.Plan(["edition", "gone"], [edition, gone], new MachineProfile(26200, "Professional"),
            t => t.Id == "gone" ? TweakState.Unavailable : TweakState.NotApplied);

        Assert.Equal(["edition", "gone"], plan.NotSupported);
        Assert.Contains("Enterprise", plan.Reasons["edition"], StringComparison.Ordinal);
        Assert.Contains("not on this PC", plan.Reasons["gone"], StringComparison.Ordinal);
    }
}
