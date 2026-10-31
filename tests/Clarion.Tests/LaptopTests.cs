using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Core.Profiles;
using Clarion.Core.SystemInfo;

namespace Clarion.Tests;

public sealed class LaptopTests
{
    private static readonly MachineProfile Desktop = new(26100, "Professional");
    private static readonly MachineProfile Laptop = new(26100, "Professional") { Traits = new MachineTraits { IsPortable = true, HasBattery = true } };

    private static Tweak Make(string id, PortableFit fit = PortableFit.Any, string note = "") => new()
    {
        Id = id,
        Category = "T",
        Name = id,
        Summary = "s",
        What = "w",
        Benefit = "b",
        Risk = "r",
        Evidence = Evidence.Proven,
        RiskLevel = RiskLevel.Safe,
        Portable = fit,
        PortableNote = note,
        Apply = [new SetRegistryValue(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", id), new RegistryData(RegistryKind.DWord, "1"))],
    };

    // One SMBIOS table made by hand: the header, a System Information structure naming the maker, then a Chassis structure.
    private static byte[] Table(byte chassisByte, string maker = "ASUSTeK COMPUTER INC.")
    {
        var body = new List<byte>();
        body.AddRange([1, 8, 0, 0, 1, 0, 0, 0]);
        body.AddRange(System.Text.Encoding.ASCII.GetBytes(maker));
        body.AddRange([0, 0]);
        body.AddRange([3, 9, 1, 0, 0, chassisByte, 0, 0, 0]);
        body.AddRange([0, 0]);
        body.AddRange([127, 4, 0, 0, 0, 0]);
        var raw = new List<byte> { 0, 3, 4, 0 };
        raw.AddRange(BitConverter.GetBytes(body.Count));
        raw.AddRange(body);
        return raw.ToArray();
    }

    [Theory]
    [InlineData(3, false)]
    [InlineData(6, false)]
    [InlineData(13, false)]
    [InlineData(8, true)]
    [InlineData(9, true)]
    [InlineData(10, true)]
    [InlineData(11, true)]
    [InlineData(14, true)]
    [InlineData(30, true)]
    [InlineData(31, true)]
    [InlineData(32, true)]
    public void Chassis_type_decides_portable(int chassis, bool portable) =>
        Assert.Equal(portable, MachineTraits.Classify([chassis], false, [], "").IsPortable);

    [Fact]
    public void A_battery_makes_a_pc_portable_even_when_the_chassis_says_other()
    {
        var traits = MachineTraits.Classify([1], true, [], "");
        Assert.True(traits.IsPortable);
        Assert.True(traits.HasBattery);
    }

    [Fact]
    public void A_desktop_with_two_graphics_adapters_is_not_hybrid()
    {
        var traits = MachineTraits.Classify([3], false, ["NVIDIA GeForce RTX 4090", "AMD Radeon(TM) Graphics"], "");
        Assert.False(traits.IsPortable);
        Assert.False(traits.HasHybridGraphics);
    }

    [Fact]
    public void A_laptop_with_an_integrated_and_a_discrete_adapter_is_hybrid()
    {
        var traits = MachineTraits.Classify([10], true, ["Intel(R) Iris(R) Xe Graphics", "NVIDIA GeForce RTX 4060 Laptop GPU"], "");
        Assert.True(traits.HasHybridGraphics);
    }

    [Theory]
    [InlineData("Microsoft Basic Display Adapter")]
    [InlineData("Parsec Virtual Display Adapter")]
    [InlineData("Microsoft Remote Display Adapter")]
    public void Virtual_adapters_do_not_make_a_laptop_hybrid(string virtualName)
    {
        var traits = MachineTraits.Classify([10], true, ["NVIDIA GeForce RTX 4060 Laptop GPU", virtualName], "");
        Assert.False(traits.HasHybridGraphics);
    }

    [Fact]
    public void Smbios_table_gives_the_chassis_and_the_maker()
    {
        var raw = Table(10);
        Assert.Equal([10], SmbiosTable.ChassisTypes(raw));
        Assert.Equal("ASUSTeK COMPUTER INC.", SmbiosTable.Maker(raw));
    }

    [Fact]
    public void The_lock_bit_of_the_chassis_byte_is_ignored() =>
        Assert.Equal([9], SmbiosTable.ChassisTypes(Table(0x80 | 9)));

    [Fact]
    public void A_cut_short_or_empty_table_gives_nothing_and_does_not_throw()
    {
        Assert.Empty(SmbiosTable.ChassisTypes([]));
        Assert.Empty(SmbiosTable.ChassisTypes(Table(10)[..20]));
        Assert.Equal("", SmbiosTable.Maker([1, 2, 3]));
    }

    [Fact]
    public void A_desktop_never_gets_a_laptop_warning()
    {
        var t = Make("x", PortableFit.Avoid, "Hot.");
        Assert.Null(Desktop.PortableWarning(t));
        Assert.False(Desktop.HoldsBack(t));
    }

    [Fact]
    public void A_laptop_gets_the_warning_for_caution_and_avoid_and_holds_back_only_avoid()
    {
        var caution = Make("c", PortableFit.Caution, "Careful.");
        var avoid = Make("a", PortableFit.Avoid, "Hot.");
        Assert.Equal("Careful.", Laptop.PortableWarning(caution));
        Assert.Equal("Hot.", Laptop.PortableWarning(avoid));
        Assert.False(Laptop.HoldsBack(caution));
        Assert.True(Laptop.HoldsBack(avoid));
        Assert.Null(Laptop.PortableWarning(Make("plain")));
    }

    [Fact]
    public void The_planner_leaves_out_avoid_settings_on_a_laptop_and_says_why()
    {
        var catalog = new[] { Make("plain"), Make("hot", PortableFit.Avoid, "Runs hot."), Make("care", PortableFit.Caution, "Careful.") };
        var plan = CliPlanner.Plan(["plain", "hot", "care"], catalog, Laptop, _ => TweakState.NotApplied);

        Assert.Equal(["plain", "care"], plan.ToApply.Select(t => t.Id));
        Assert.Equal("Runs hot.", plan.HeldBack["hot"]);
    }

    [Fact]
    public void The_planner_includes_them_when_asked_and_on_a_desktop()
    {
        var catalog = new[] { Make("hot", PortableFit.Avoid, "Runs hot.") };
        Assert.Equal(["hot"], CliPlanner.Plan(["hot"], catalog, Laptop, _ => TweakState.NotApplied, allowOnLaptop: true).ToApply.Select(t => t.Id));
        var onDesktop = CliPlanner.Plan(["hot"], catalog, Desktop, _ => TweakState.NotApplied);
        Assert.Equal(["hot"], onDesktop.ToApply.Select(t => t.Id));
        Assert.Empty(onDesktop.HeldBack);
    }

    [Fact]
    public void A_setting_that_is_already_on_is_not_reported_as_held_back()
    {
        var catalog = new[] { Make("hot", PortableFit.Avoid, "Runs hot.") };
        var plan = CliPlanner.Plan(["hot"], catalog, Laptop, _ => TweakState.Applied);
        Assert.Empty(plan.HeldBack);
        Assert.Equal(["hot"], plan.AlreadyOn);
    }

    [Fact]
    public void Every_laptop_flag_in_the_catalog_has_a_plain_note()
    {
        foreach (var t in CatalogLoader.LoadEmbedded().Where(t => t.Portable != PortableFit.Any))
        {
            Assert.True(t.PortableNote.Length >= 40, $"{t.Id} is flagged for laptops and needs a note that says why");
            Assert.DoesNotContain(';', t.PortableNote);
            Assert.DoesNotContain('\u2014', t.PortableNote);
        }
    }

    [Fact]
    public void A_note_without_a_flag_is_a_mistake()
    {
        foreach (var t in CatalogLoader.LoadEmbedded().Where(t => t.Portable == PortableFit.Any))
            Assert.True(t.PortableNote.Length == 0, $"{t.Id} has a laptop note but no flag");
    }

    [Theory]
    [InlineData("power.plan-high-performance", PortableFit.Avoid)]
    [InlineData("power.plan-ultimate", PortableFit.Avoid)]
    [InlineData("gpu.nvidia-max-performance", PortableFit.Avoid)]
    [InlineData("power.hibernate-off", PortableFit.Caution)]
    [InlineData("privacy.location-off", PortableFit.Caution)]
    [InlineData("sync.find-my-device-off", PortableFit.Caution)]
    public void The_settings_that_hurt_a_laptop_are_flagged(string id, PortableFit expected) =>
        Assert.Equal(expected, CatalogLoader.LoadEmbedded().Single(t => t.Id == id).Portable);

    [Fact]
    public void No_preset_pushes_an_avoid_setting_on_a_laptop()
    {
        var catalog = CatalogLoader.LoadEmbedded();
        foreach (var (name, ids) in CatalogLoader.LoadPresets())
        {
            var plan = CliPlanner.Plan(ids, catalog, Laptop, _ => TweakState.NotApplied);
            Assert.DoesNotContain(plan.ToApply, t => t.Portable == PortableFit.Avoid);
        }
    }

    [Theory]
    [InlineData("RealtekSemiconductorCorp.RealtekAudioControl")]
    [InlineData("DolbyLaboratories.DolbyAccess")]
    [InlineData("NVIDIACorp.NVIDIAControlPanel")]
    [InlineData("AppUp.IntelGraphicsExperience")]
    [InlineData("AdvancedMicroDevicesInc-2.AMDRadeonSoftware")]
    public void Audio_and_graphics_driver_apps_are_never_removed(string package) =>
        Assert.True(Clarion.Core.Appx.AppxSafety.IsProtected(package));
}
