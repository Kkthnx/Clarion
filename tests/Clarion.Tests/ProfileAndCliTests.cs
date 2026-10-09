using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Core.Profiles;

namespace Clarion.Tests;

public sealed class ProfileAndCliTests
{
    [Fact]
    public void A_saved_setup_round_trips_and_sorts_and_dedupes_ids()
    {
        var json = ProfileFile.Serialize(["b.two", "a.one", "B.TWO"], "0.1.0");
        var load = ProfileFile.Parse(json);
        Assert.Null(load.Error);
        Assert.Equal(["a.one", "b.two"], load.Profile!.Tweaks);
        Assert.Equal("Clarion", load.Profile.App);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    [InlineData("{\"format\":1,\"app\":\"Other\",\"version\":\"1\",\"created\":\"2026-01-01T00:00:00Z\",\"tweaks\":[]}")]
    [InlineData("{\"format\":99,\"app\":\"Clarion\",\"version\":\"1\",\"created\":\"2026-01-01T00:00:00Z\",\"tweaks\":[]}")]
    [InlineData("{\"format\":1,\"app\":\"Clarion\",\"version\":\"1\",\"created\":\"2026-01-01T00:00:00Z\",\"tweaks\":[\"ok.id\",\"bad id; calc\"]}")]
    [InlineData("{\"format\":1,\"app\":\"Clarion\",\"version\":\"1\",\"created\":\"2026-01-01T00:00:00Z\",\"tweaks\":null}")]
    public void Bad_files_never_throw_and_always_explain(string json)
    {
        var load = ProfileFile.Parse(json);
        Assert.Null(load.Profile);
        Assert.False(string.IsNullOrWhiteSpace(load.Error));
    }

    [Fact]
    public void Oversized_files_and_missing_files_are_refused_gracefully()
    {
        Assert.NotNull(ProfileFile.Parse(new string(' ', (int)ProfileFile.MaxBytes + 1)).Error);
        Assert.NotNull(ProfileFile.Load(Path.Combine(Path.GetTempPath(), "clarion-missing-" + Guid.NewGuid().ToString("N") + ".json")).Error);
    }

    [Fact]
    public void Too_many_ids_are_refused()
    {
        var ids = Enumerable.Range(0, ProfileFile.MaxIds + 1).Select(i => $"id.{i}");
        Assert.NotNull(ProfileFile.Parse(ProfileFile.Serialize(ids, "1")).Error);
    }

    [Theory]
    [InlineData("--apply a.json", CliMode.Apply)]
    [InlineData("--apply-preset standard --preview", CliMode.Apply)]
    [InlineData("--clean --only shaders.nvidia,windows.temp", CliMode.Clean)]
    [InlineData("--list-presets", CliMode.ListPresets)]
    [InlineData("--version", CliMode.Version)]
    [InlineData("--help", CliMode.Help)]
    [InlineData("", CliMode.None)]
    public void Arguments_pick_the_right_mode(string line, CliMode expected)
    {
        var o = CliOptions.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Assert.Equal(expected, o.Mode);
        Assert.Equal(expected != CliMode.None, o.IsCli);
    }

    [Theory]
    [InlineData("--apply")]
    [InlineData("--apply --preview")]
    [InlineData("--apply-preset")]
    [InlineData("--frobnicate")]
    public void Missing_values_and_unknown_switches_are_errors_not_silent(string line)
    {
        var o = CliOptions.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        Assert.NotNull(o.Error);
    }

    [Theory]
    [InlineData(0, "older Clarion")]
    [InlineData(99, "newer Clarion")]
    public void A_setup_file_from_another_format_fails_loudly_and_says_which_way(int format, string expected)
    {
        var json = "{\"format\":" + format + ",\"app\":\"Clarion\",\"version\":\"0.0.1\",\"created\":\"2026-01-01T00:00:00Z\",\"tweaks\":[\"ok.id\"]}";

        var load = ProfileFile.Parse(json);

        Assert.Null(load.Profile);
        Assert.Contains(expected, load.Error);
    }

    [Fact]
    public void A_saved_setup_file_carries_the_current_format()
    {
        var load = ProfileFile.Parse(ProfileFile.Serialize(["a.b"], "1.0"));
        Assert.Equal(SetupProfile.CurrentFormat, load.Profile!.Format);
    }

    [Fact]
    public void The_enable_protection_switch_is_read_and_off_by_default()
    {
        Assert.True(CliOptions.Parse(["--apply", "s.json", "--enable-protection"]).EnableProtection);
        Assert.False(CliOptions.Parse(["--apply", "s.json"]).EnableProtection);
        Assert.Contains("--enable-protection", CliOptions.HelpText);
    }

    [Fact]
    public void Switches_and_lists_are_read()
    {
        var o = CliOptions.Parse(["--clean", "--only", "a, b", "--include", "c", "--preview", "--no-restore-point"]);
        Assert.Equal(["a", "b"], o.Only);
        Assert.Equal(["c"], o.Include);
        Assert.True(o.Preview);
        Assert.True(o.NoRestorePoint);
    }

    private static Tweak Make(string id, string? group = null, int minBuild = 0) => new()
    {
        Id = id, Category = "T", Name = id, Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe, ExclusiveGroup = group,
        Requires = new Requirements { MinBuild = minBuild },
        Apply = [new SetRegistryValue(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", id), new RegistryData(RegistryKind.DWord, "1"))],
    };

    [Fact]
    public void Plan_sorts_ids_into_apply_already_unsupported_and_unknown()
    {
        var catalog = new[] { Make("a"), Make("b"), Make("c", minBuild: 99999), Make("d") };
        var state = new Dictionary<string, TweakState> { ["a"] = TweakState.NotApplied, ["b"] = TweakState.Applied, ["d"] = TweakState.Unavailable };

        var plan = CliPlanner.Plan(["a", "b", "c", "d", "zzz", "A"], catalog, new MachineProfile(26100, "Professional"), t => state[t.Id]);

        Assert.Equal(["a"], plan.ToApply.Select(t => t.Id));
        Assert.Equal(["b"], plan.AlreadyOn);
        Assert.Equal(["c", "d"], plan.NotSupported);
        Assert.Equal(["zzz"], plan.Unknown);
    }

    [Fact]
    public void Plan_keeps_one_choice_per_exclusive_group()
    {
        var catalog = new[] { Make("p1", "plan"), Make("p2", "plan"), Make("x") };
        var plan = CliPlanner.Plan(["p1", "p2", "x"], catalog, new MachineProfile(26100, "Professional"), _ => TweakState.NotApplied);
        Assert.Equal(["p2", "x"], plan.ToApply.Select(t => t.Id));
    }

    [Fact]
    public void Every_embedded_preset_resolves_to_real_settings_through_the_planner()
    {
        var catalog = CatalogLoader.LoadEmbedded();
        foreach (var (name, ids) in CatalogLoader.LoadPresets())
        {
            var plan = CliPlanner.Plan(ids, catalog, new MachineProfile(26100, "Professional"), _ => TweakState.NotApplied);
            Assert.Empty(plan.Unknown);
            Assert.True(plan.ToApply.Count > 0, name);
        }
    }
}
