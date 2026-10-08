using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Power;

namespace Clarion.Tests;

public sealed class FakePowerStore : IPowerStore
{
    public List<PowerPlan> Plans { get; } = [];
    public bool? Hibernation { get; set; } = true;
    public List<string> Calls { get; } = [];
    public bool FailDuplicate { get; set; }

    public void Add(string guid, string name, bool active = false) => Plans.Add(new PowerPlan(guid, name, active));

    public IReadOnlyList<PowerPlan> List() => Plans.ToList();

    public void SetActive(string guid)
    {
        Calls.Add($"active {guid}");
        for (var i = 0; i < Plans.Count; i++) Plans[i] = Plans[i] with { IsActive = Plans[i].Guid == guid };
    }

    public string Duplicate(string templateGuid)
    {
        Calls.Add($"duplicate {templateGuid}");
        if (FailDuplicate) throw new InvalidOperationException("not offered");
        const string id = "11111111-2222-3333-4444-555555555555";
        Plans.Add(new PowerPlan(id, "Ultimate Performance", false));
        return id;
    }

    public void Rename(string guid, string name)
    {
        Calls.Add($"rename {guid} {name}");
        var i = Plans.FindIndex(p => p.Guid == guid);
        Plans[i] = Plans[i] with { Name = name };
    }

    public bool? IsHibernationEnabled() => Hibernation;
    public void SetHibernation(bool enabled) { Calls.Add($"hibernate {enabled}"); Hibernation = enabled; }
}

public sealed class PowerTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-pw-" + Guid.NewGuid().ToString("N"));
    private readonly FakePowerStore _store = new();
    private readonly TweakEngine _engine;

    public PowerTests()
    {
        _store.Add(PowerPlans.Balanced, "Balanced", active: true);
        _store.Add(PowerPlans.PowerSaver, "Power saver");
        _engine = new TweakEngine([new PowerHandler(_store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak Make(params Operation[] ops) => new()
    {
        Id = "pw.one", Category = "Power", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Situational, RiskLevel = RiskLevel.Low, Scope = TweakScope.Machine, Apply = ops,
    };

    [Fact]
    public void Switching_plan_and_reverting_goes_back_to_the_previous_plan()
    {
        var tweak = Make(new SetPowerPlan("power-saver"));
        Assert.Equal(TweakState.NotApplied, _engine.Detect(tweak));
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));
        Assert.Equal(PowerPlans.PowerSaver, _store.List().Single(p => p.IsActive).Guid);

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Equal(PowerPlans.Balanced, _store.List().Single(p => p.IsActive).Guid);
    }

    [Fact]
    public void Ultimate_is_created_renamed_and_activated_when_missing()
    {
        var tweak = Make(new SetPowerPlan("ultimate"));
        Assert.Equal(TweakState.NotApplied, _engine.Detect(tweak));

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);

        Assert.Contains($"duplicate {PowerPlans.UltimateTemplate}", _store.Calls);
        Assert.Contains(_store.Calls, c => c.StartsWith("rename ", StringComparison.Ordinal) && c.EndsWith("Ultimate Performance", StringComparison.Ordinal));
        Assert.Equal("Ultimate Performance", _store.List().Single(p => p.IsActive).Name);
    }

    [Fact]
    public void An_existing_ultimate_plan_is_reused_not_duplicated()
    {
        _store.Add("99999999-8888-7777-6666-555555555555", "Ultimate Performance");
        Assert.True(_engine.Apply(Make(new SetPowerPlan("ultimate")), Guid.NewGuid()).Success);
        Assert.DoesNotContain(_store.Calls, c => c.StartsWith("duplicate", StringComparison.Ordinal));
    }

    [Fact]
    public void A_pc_that_refuses_the_ultimate_plan_fails_cleanly()
    {
        _store.FailDuplicate = true;
        var result = _engine.Apply(Make(new SetPowerPlan("ultimate")), Guid.NewGuid());
        Assert.False(result.Success);
        Assert.Equal(PowerPlans.Balanced, _store.List().Single(p => p.IsActive).Guid);
    }

    [Fact]
    public void A_missing_builtin_plan_makes_the_tweak_unavailable()
    {
        Assert.Equal(TweakState.Unavailable, _engine.Detect(Make(new SetPowerPlan("high-performance"))));
    }

    [Fact]
    public void Hibernation_off_and_revert()
    {
        var tweak = Make(new SetHibernation(false));
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.False(_store.Hibernation);
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.True(_store.Hibernation);
    }

    [Fact]
    public void List_output_parses_in_any_language_and_marks_the_active_plan()
    {
        const string english = """
        Existing Power Schemes (* Active)
        -----------------------------------
        Power Scheme GUID: 381b4222-f694-41f0-9685-ff5bb260df2e  (Balanced)
        Power Scheme GUID: e4d71f4b-dcdb-49b6-9c6b-170cc4cb5095  (Ultimate Performance) *
        """;
        const string german = """
        GUID des Energieschemas: 381b4222-f694-41f0-9685-ff5bb260df2e  (Ausbalanciert) *
        GUID des Energieschemas: a1841308-3541-4fab-bc81-f71556f20b4a  (Energiesparmodus)
        """;
        var en = PowerPlans.ParseList(english);
        Assert.Equal(2, en.Count);
        Assert.Equal("Ultimate Performance", en.Single(p => p.IsActive).Name);
        var de = PowerPlans.ParseList(german);
        Assert.Equal(PowerPlans.Balanced, de.Single(p => p.IsActive).Guid);
    }

    [Theory]
    [InlineData("balanced", true)]
    [InlineData("ultimate", true)]
    [InlineData("e4d71f4b-dcdb-49b6-9c6b-170cc4cb5095", true)]
    [InlineData("x; calc", false)]
    [InlineData("", false)]
    public void Plan_names_are_validated(string plan, bool valid) => Assert.Equal(valid, PowerPlans.IsValid(plan));
}
