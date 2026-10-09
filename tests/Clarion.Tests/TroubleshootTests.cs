using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Troubleshoot;

namespace Clarion.Tests;

public sealed class TroubleshootTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-trouble-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();
    private readonly ChangeJournal _journal;
    private readonly TweakEngine _engine;
    private readonly Troubleshooter _shooter;
    private long _tick;

    // Topics where no one would describe the result as something that stopped working.
    private static readonly string[] SilentTopics = ["Diagnostics", "Suggestions and ads", "Defender sharing"];
    private static readonly string[] SilentTweaks = ["system.long-paths"];

    public TroubleshootTests()
    {
        _journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        _engine = new TweakEngine([new RegistryHandler(_reg)], _journal, new StepClock(() => ++_tick));
        _shooter = new Troubleshooter(_journal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private sealed class StepClock(Func<long> next) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMinutes(next());
    }

    private static Tweak Make(string id, string topic) => new()
    {
        Id = id, Category = "Test", Topic = topic, Name = id, Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe,
        Apply = [new SetRegistryValue(new RegistryTarget(RegistryHive.CurrentUser, "Software\\Test", id), new RegistryData(RegistryKind.DWord, "0"))],
    };

    private static Symptom Sym(string[] topics, string[] ids) => new("s", "t", "tip", topics, ids);

    [Fact]
    public void The_symptom_list_matches_the_catalog()
    {
        Assert.Empty(Troubleshooter.Validate(CatalogLoader.LoadSymptoms(), CatalogLoader.LoadEmbedded()));
    }

    [Fact]
    public void Every_setting_can_be_found_from_some_symptom()
    {
        var symptoms = CatalogLoader.LoadSymptoms();
        var named = symptoms.SelectMany(s => s.TweakIds).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var topics = symptoms.SelectMany(s => s.Topics).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var orphans = CatalogLoader.LoadEmbedded()
            .Where(t => !named.Contains(t.Id) && !topics.Contains(t.Topic) && !SilentTopics.Contains(t.Topic, StringComparer.OrdinalIgnoreCase) && !SilentTweaks.Contains(t.Id))
            .Select(t => $"{t.Id} ({t.Topic})")
            .ToList();

        Assert.Empty(orphans);
    }

    [Fact]
    public void Validation_catches_unknown_topics_and_settings()
    {
        var bad = new Symptom("x", "t", "tip", ["Nope"], ["no.such"]);
        var errors = Troubleshooter.Validate([bad], [Make("a", "A")]);
        Assert.Contains(errors, e => e.Contains("unknown topic"));
        Assert.Contains(errors, e => e.Contains("unknown setting"));
    }

    [Fact]
    public void Only_settings_that_are_still_on_are_suspects()
    {
        var a = Make("a", "Search");
        var b = Make("b", "Search");
        _engine.Apply(a, Guid.NewGuid());
        _engine.Apply(b, Guid.NewGuid());
        _engine.Revert(b, Guid.NewGuid());

        var suspects = _shooter.Suspects(Sym(["Search"], []), [a, b]);

        Assert.Equal("a", Assert.Single(suspects).Tweak.Id);
    }

    [Fact]
    public void A_setting_never_applied_is_not_a_suspect()
    {
        var suspects = _shooter.Suspects(Sym(["Search"], []), [Make("a", "Search")]);
        Assert.Empty(suspects);
    }

    [Fact]
    public void Named_settings_come_first_then_the_newest_change()
    {
        var oldTopic = Make("old", "Search");
        var named = Make("named", "Other");
        var newTopic = Make("new", "Search");
        _engine.Apply(oldTopic, Guid.NewGuid());
        _engine.Apply(named, Guid.NewGuid());
        _engine.Apply(newTopic, Guid.NewGuid());

        var suspects = _shooter.Suspects(Sym(["Search"], ["named"]), [oldTopic, named, newTopic]);

        Assert.Equal(["named", "new", "old"], suspects.Select(s => s.Tweak.Id));
        Assert.True(suspects[0].Direct);
        Assert.False(suspects[1].Direct);
    }

    [Fact]
    public void A_setting_released_from_tracking_is_not_a_suspect()
    {
        var a = Make("a", "Search");
        _engine.Apply(a, Guid.NewGuid());
        _engine.Release(a, Guid.NewGuid());

        Assert.Empty(_shooter.Suspects(Sym(["Search"], []), [a]));
    }
}
