using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class RegistryListTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-rl-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();
    private readonly ChangeJournal _journal;
    private readonly TweakEngine _engine;

    private static readonly RegistryTarget Target = new(RegistryHive.CurrentUser, "Software\\Test\\DirectX", "Global");

    public RegistryListTests()
    {
        _journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        _engine = new TweakEngine([new RegistryHandler(_reg), new RegistryListHandler(_reg)], _journal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak Make(string key = "SwapEffectUpgradeEnable", string value = "1") => new()
    {
        Id = "t.list", Category = "System", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Situational, RiskLevel = RiskLevel.Low, Apply = [new SetRegistryListEntry(Target, key, value)],
    };

    private string? Text() => _reg.Read(Target).Data?.Value;

    [Theory]
    [InlineData(null, "SwapEffectUpgradeEnable", "1", "SwapEffectUpgradeEnable=1;")]
    [InlineData("", "A", "1", "A=1;")]
    [InlineData("VRROptimizeEnable=0;", "SwapEffectUpgradeEnable", "1", "VRROptimizeEnable=0;SwapEffectUpgradeEnable=1;")]
    [InlineData("SwapEffectUpgradeEnable=0;VRROptimizeEnable=0;", "SwapEffectUpgradeEnable", "1", "SwapEffectUpgradeEnable=1;VRROptimizeEnable=0;")]
    [InlineData("a=1;A=2;b=3", "a", "9", "a=9;b=3;")]
    [InlineData("junk;;=x;b=3;", "c", "4", "b=3;c=4;")]
    public void Setting_an_entry_keeps_the_others_and_its_own_place(string? before, string key, string value, string expected) =>
        Assert.Equal(expected, RegistryList.Set(before, key, value));

    [Fact]
    public void Entries_are_found_without_regard_to_case_and_spacing()
    {
        Assert.Equal("1", RegistryList.Get(" swapeffectupgradeenable = 1 ; B=2;", "SwapEffectUpgradeEnable"));
        Assert.Null(RegistryList.Get("B=2;", "A"));
    }

    [Fact]
    public void Apply_adds_the_entry_without_touching_the_ones_windows_wrote()
    {
        _reg.Write(Target, new RegistryData(RegistryKind.String, "VRROptimizeEnable=0;"));
        var tweak = Make();

        Assert.Equal(TweakState.NotApplied, _engine.Detect(tweak));
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Equal("VRROptimizeEnable=0;SwapEffectUpgradeEnable=1;", Text());
        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));
    }

    [Fact]
    public void Revert_puts_back_the_whole_text_that_was_there()
    {
        _reg.Write(Target, new RegistryData(RegistryKind.String, "VRROptimizeEnable=0;SwapEffectUpgradeEnable=0;"));
        var tweak = Make();

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.Equal("VRROptimizeEnable=0;SwapEffectUpgradeEnable=0;", Text());
    }

    [Fact]
    public void Revert_removes_a_value_that_did_not_exist_and_the_keys_it_made()
    {
        var tweak = Make();

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_reg.HasKey(RegistryHive.CurrentUser, "Software\\Test\\DirectX"));
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.False(_reg.Read(Target).Exists);
    }

    [Fact]
    public void A_value_of_another_kind_is_left_alone()
    {
        _reg.Write(Target, new RegistryData(RegistryKind.DWord, "7"));

        var result = _engine.Apply(Make(), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("7", Text());
    }

    [Fact]
    public void An_entry_in_the_catalog_must_be_plain_text()
    {
        var bad = Make("Bad Name;x", "1;2") with { Topic = "t", Facts = ["one", "two"], Advice = "a" };
        var problems = CatalogLoader.Validate([bad]);
        Assert.Contains(problems, e => e.Contains("not a plain list entry name"));
        Assert.Contains(problems, e => e.Contains("not a plain list entry value"));
    }

    [Fact]
    public void A_list_entry_in_the_current_users_hive_belongs_to_that_account()
    {
        Assert.True(ChangeJournal.IsPerUser(new SetRegistryListEntry(Target, "A", "1")));
        Assert.False(ChangeJournal.IsPerUser(new SetRegistryListEntry(Target with { Hive = RegistryHive.LocalMachine }, "A", "1")));
    }

    [Fact]
    public void The_operation_round_trips_through_json()
    {
        Operation op = new SetRegistryListEntry(Target, "SwapEffectUpgradeEnable", "1");
        var json = System.Text.Json.JsonSerializer.Serialize(op, CatalogLoader.Options);
        Assert.Contains("registry.list-entry", json);
        Assert.Equal(op, System.Text.Json.JsonSerializer.Deserialize<Operation>(json, CatalogLoader.Options));
    }
}
