using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Nvidia;

namespace Clarion.Tests;

public sealed class FakeNvidiaStore : INvidiaDriverStore
{
    public bool Available { get; set; } = true;
    public Dictionary<uint, uint> Stored { get; } = [];
    public List<string> Calls { get; } = [];

    public bool IsAvailable() => Available;
    public uint? Read(uint id) => Stored.TryGetValue(id, out var v) ? v : null;
    public void Write(uint id, uint value) { Calls.Add($"write {id:X8} {value}"); Stored[id] = value; }
    public void Reset(uint id) { Calls.Add($"reset {id:X8}"); Stored.Remove(id); }
}

public sealed class NvidiaTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-nv-" + Guid.NewGuid().ToString("N"));
    private readonly FakeNvidiaStore _store = new();
    private readonly ChangeJournal _journal;
    private readonly TweakEngine _engine;

    public NvidiaTests()
    {
        _journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        _engine = new TweakEngine([new NvidiaHandler(_store)], _journal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak Make(params Operation[] ops) => new()
    {
        Id = "gpu.test",
        Category = "System",
        Name = "n",
        Summary = "s",
        What = "w",
        Benefit = "b",
        Risk = "r",
        Evidence = Evidence.Situational,
        RiskLevel = RiskLevel.Low,
        Scope = TweakScope.Machine,
        Apply = ops,
    };

    private static readonly SetNvidiaSetting LowLatency = new(NvidiaSettings.MaxPreRenderedFrames, 1);

    [Fact]
    public void Applying_sets_the_value_and_reverting_puts_back_the_one_that_was_stored()
    {
        _store.Stored[NvidiaSettings.MaxPreRenderedFrames] = 3;
        var tweak = Make(LowLatency);

        Assert.Equal(TweakState.NotApplied, _engine.Detect(tweak));
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Equal(1u, _store.Stored[NvidiaSettings.MaxPreRenderedFrames]);
        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal(3u, _store.Stored[NvidiaSettings.MaxPreRenderedFrames]);
    }

    [Fact]
    public void A_value_that_was_not_stored_is_taken_out_again_on_revert_not_written_as_a_number()
    {
        var tweak = Make(LowLatency);

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.DoesNotContain(NvidiaSettings.MaxPreRenderedFrames, _store.Stored.Keys);
        Assert.Equal(["write 007BA09E 1", "reset 007BA09E"], _store.Calls);
    }

    [Fact]
    public void Without_an_nvidia_card_the_setting_is_not_available_and_nothing_is_written()
    {
        _store.Available = false;
        var tweak = Make(LowLatency);

        Assert.Equal(TweakState.Unavailable, _engine.Detect(tweak));
        Assert.False(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Empty(_store.Calls);
    }

    [Fact]
    public void A_setting_applied_outside_clarion_is_reset_to_the_driver_default_by_the_catalog_undo()
    {
        _store.Stored[NvidiaSettings.MaxPreRenderedFrames] = 1;
        var tweak = Make(LowLatency) with { Undo = [new ResetNvidiaSetting(NvidiaSettings.MaxPreRenderedFrames)] };

        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Empty(_store.Stored);
    }

    [Fact]
    public void A_setting_that_is_not_on_the_list_is_refused_even_from_a_recorded_step()
    {
        var handler = new NvidiaHandler(_store);

        Assert.Throws<InvalidOperationException>(() => handler.Execute(new SetNvidiaSetting(0x20C1221E, 2)));
        Assert.Throws<InvalidOperationException>(() => handler.Execute(new ResetNvidiaSetting(0xDEADBEEF)));
        Assert.Empty(_store.Calls);
    }

    [Fact]
    public void Undo_may_put_back_a_number_the_catalog_never_offered()
    {
        var handler = new NvidiaHandler(_store);

        handler.Execute(new SetNvidiaSetting(NvidiaSettings.ShaderCacheMaxSize, 5120));

        Assert.Equal(5120u, _store.Stored[NvidiaSettings.ShaderCacheMaxSize]);
    }

    [Fact]
    public void The_catalog_only_changes_listed_settings_to_values_nvidia_lists_for_them()
    {
        var errors = CatalogLoader.Validate(CatalogLoader.LoadEmbedded().Where(t => t.Id.StartsWith("gpu.nvidia", StringComparison.Ordinal)));
        Assert.Empty(errors);

        var bad = Make(new SetNvidiaSetting(NvidiaSettings.PowerManagementMode, 9), new SetNvidiaSetting(0x20C1221E, 1), new ResetNvidiaSetting(0x1));
        var problems = CatalogLoader.Validate([bad with { Topic = "t", Facts = ["one", "two"], Advice = "a" }]);
        Assert.Contains(problems, e => e.Contains("not a value NVIDIA lists"));
        Assert.Contains(problems, e => e.Contains("is not one .Clarion changes"));
    }

    [Fact]
    public void Every_nvidia_setting_in_the_catalog_has_a_default_to_go_back_to()
    {
        foreach (var t in CatalogLoader.LoadEmbedded().Where(t => t.Apply.Any(o => o is SetNvidiaSetting)))
        {
            Assert.All(t.Apply.OfType<SetNvidiaSetting>(), s => Assert.Contains(t.Undo.OfType<ResetNvidiaSetting>(), r => r.Id == s.Id));
        }
    }

    [Fact]
    public void Operations_describe_themselves_with_the_name_the_driver_uses()
    {
        Assert.Contains("Maximum pre-rendered frames", LowLatency.Describe());
        Assert.Contains("1 frame", LowLatency.Describe());
        Assert.Contains("no limit", new SetNvidiaSetting(NvidiaSettings.ShaderCacheMaxSize, NvidiaSettings.Unlimited).Describe());
        Assert.Contains("default", new ResetNvidiaSetting(NvidiaSettings.TextureFilteringQuality).Describe());
    }

    [Fact]
    public void The_operations_round_trip_through_json()
    {
        var json = System.Text.Json.JsonSerializer.Serialize<Operation>(LowLatency, CatalogLoader.Options);
        Assert.Contains("nvidia.set", json);
        Assert.Equal(LowLatency, System.Text.Json.JsonSerializer.Deserialize<Operation>(json, CatalogLoader.Options));
    }
}
