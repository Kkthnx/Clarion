using Clarion.Core.Abstractions;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Nvidia;
using Clarion.Core.Power;

namespace Clarion.Tests;

/// <summary>
/// Reading every setting at start up has to be quick, so what is slow to ask for is asked once and kept. What is kept must never decide
/// a change, and that is what these tests hold the handlers and the runner to.
/// </summary>
public sealed class ReadCachingTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-cache-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak Make(string id, params Operation[] ops) => new()
    {
        Id = id,
        Category = "System",
        Name = id,
        Summary = "s",
        What = "w",
        Benefit = "b",
        Risk = "r",
        Evidence = Evidence.Situational,
        RiskLevel = RiskLevel.Low,
        Scope = TweakScope.Machine,
        Apply = ops,
    };

    private sealed class CountingNvidiaStore : INvidiaDriverStore
    {
        public Dictionary<uint, uint> Stored { get; } = [];
        public int Reads { get; private set; }
        public int ManyReads { get; private set; }

        public bool IsAvailable() => true;

        public uint? Read(uint id) { Reads++; return Stored.TryGetValue(id, out var v) ? v : null; }

        public IReadOnlyDictionary<uint, uint?> ReadMany(IEnumerable<uint> ids)
        {
            ManyReads++;
            return ids.Distinct().ToDictionary(id => id, id => Stored.TryGetValue(id, out var v) ? (uint?)v : null);
        }

        public void Write(uint id, uint value) => Stored[id] = value;

        public void Reset(uint id) => Stored.Remove(id);
    }

    private sealed class CountingPowerStore : IPowerStore
    {
        public List<PowerPlan> Plans { get; } = [new(PowerPlans.Balanced, "Balanced", true), new(PowerPlans.HighPerformance, "High performance", false)];
        public int Lists { get; private set; }
        public int Activations { get; private set; }

        public IReadOnlyList<PowerPlan> List() { Lists++; return Plans.ToList(); }

        public void SetActive(string guid)
        {
            Activations++;
            for (var i = 0; i < Plans.Count; i++) Plans[i] = Plans[i] with { IsActive = Plans[i].Guid == guid };
        }

        public string Duplicate(string templateGuid) => throw new NotSupportedException();
        public void Rename(string guid, string name) => throw new NotSupportedException();
        public bool? IsHibernationEnabled() => true;
        public void SetHibernation(bool enabled) { }
    }

    private TweakEngine EngineFor(params IOperationHandler[] handlers) => new(handlers, new ChangeJournal(Path.Combine(_dir, "j.jsonl")));

    [Fact]
    public void The_nvidia_settings_the_catalog_reads_are_read_in_one_go_and_not_one_at_a_time()
    {
        var store = new CountingNvidiaStore();
        store.Stored[NvidiaSettings.MaxPreRenderedFrames] = 1;
        var engine = EngineFor(new NvidiaHandler(store));
        var tweaks = new[]
        {
            Make("a", new SetNvidiaSetting(NvidiaSettings.MaxPreRenderedFrames, 1)),
            Make("b", new SetNvidiaSetting(NvidiaSettings.MaxPreRenderedFrames, 2)),
        };

        engine.WarmUp(tweaks);

        Assert.Equal(TweakState.Applied, engine.Detect(tweaks[0]));
        Assert.Equal(TweakState.NotApplied, engine.Detect(tweaks[1]));
        Assert.Equal(1, store.ManyReads);
        Assert.Equal(0, store.Reads);
    }

    [Fact]
    public void A_change_made_by_the_driver_app_after_the_read_is_not_overwritten_by_a_stale_undo()
    {
        var store = new CountingNvidiaStore();
        store.Stored[NvidiaSettings.MaxPreRenderedFrames] = 3;
        var engine = EngineFor(new NvidiaHandler(store));
        var tweak = Make("a", new SetNvidiaSetting(NvidiaSettings.MaxPreRenderedFrames, 1));
        engine.WarmUp([tweak]);

        // NVIDIA's own app changes the value after .Clarion read it. The undo must remember this one.
        store.Stored[NvidiaSettings.MaxPreRenderedFrames] = 4;
        Assert.True(engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.Equal(4u, store.Stored[NvidiaSettings.MaxPreRenderedFrames]);
    }

    [Fact]
    public void Writing_a_nvidia_setting_drops_what_was_kept_so_the_next_read_sees_the_new_value()
    {
        var store = new CountingNvidiaStore();
        var engine = EngineFor(new NvidiaHandler(store));
        var tweak = Make("a", new SetNvidiaSetting(NvidiaSettings.MaxPreRenderedFrames, 1));
        engine.WarmUp([tweak]);
        Assert.Equal(TweakState.NotApplied, engine.Detect(tweak));

        Assert.True(engine.Apply(tweak, Guid.NewGuid()).Success);

        Assert.Equal(TweakState.Applied, engine.Detect(tweak));
    }

    [Fact]
    public void The_power_plans_are_listed_once_for_all_the_power_settings_that_ask()
    {
        var store = new CountingPowerStore();
        var engine = EngineFor(new PowerHandler(store));
        var tweaks = new[]
        {
            Make("a", new SetPowerPlan("balanced")),
            Make("b", new SetPowerPlan("high-performance")),
        };

        engine.WarmUp(tweaks);
        foreach (var t in tweaks) engine.Detect(t);

        Assert.Equal(1, store.Lists);
    }

    [Fact]
    public void A_run_reads_again_so_a_plan_changed_in_windows_since_the_window_opened_is_not_taken_for_still_active()
    {
        var store = new CountingPowerStore();
        store.SetActive(PowerPlans.HighPerformance);
        var engine = EngineFor(new PowerHandler(store));
        var tweak = Make("a", new SetPowerPlan("high-performance"));
        engine.WarmUp([tweak]);
        Assert.Equal(TweakState.Applied, engine.Detect(tweak));

        // The person switches back to Balanced in Windows. What was read is now wrong.
        store.SetActive(PowerPlans.Balanced);
        var before = store.Activations;
        var runner = new BatchRunner(engine, new RestorePointService(new FakeRegistry(), new FakeProcessRunner()), () => true);

        var result = runner.Apply([tweak], new MachineProfile(26100, "Professional"), new BatchOptions { CreateRestorePoint = false });

        Assert.True(result.AllSucceeded);
        Assert.False(result.Items.Single().Skipped);
        Assert.Equal(before + 1, store.Activations);
        Assert.Equal(PowerPlans.HighPerformance, store.Plans.Single(p => p.IsActive).Guid);
    }
}
