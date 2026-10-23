using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

/// <summary>A value of flags: a setting owns some bits, and Windows or the person may change the others.</summary>
public sealed class RegistryBitsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-tests-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();
    private readonly ChangeJournal _journal;
    private readonly TweakEngine _engine;

    private static readonly RegistryTarget Flags = new(RegistryHive.CurrentUser, "Control Panel\\Accessibility\\StickyKeys", "Flags");

    public RegistryBitsTests()
    {
        _journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        _engine = new TweakEngine([new RegistryHandler(_reg)], _journal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static SetRegistryValue Shortcut(string value) => new(Flags, new RegistryData(RegistryKind.String, value), "4");

    private static Tweak Make(params Operation[] apply) => new()
    {
        Id = "t.flags",
        Category = "Test",
        Name = "n",
        Summary = "s",
        What = "w",
        Benefit = "b",
        Risk = "r",
        Evidence = Evidence.Cosmetic,
        RiskLevel = RiskLevel.Safe,
        Scope = TweakScope.User,
        Apply = apply,
        Undo = [Shortcut("510")],
    };

    private void Set(string value) => _reg.Write(Flags, new RegistryData(RegistryKind.String, value));

    [Theory]
    [InlineData("506")]
    [InlineData("498")] // Windows also clears the confirm bit when the shortcut is turned off in Settings
    public void A_value_with_the_owned_bit_clear_counts_as_applied_whatever_the_other_bits_are(string now)
    {
        Set(now);
        Assert.Equal(TweakState.Applied, _engine.Detect(Make(Shortcut("506"))));
    }

    [Fact]
    public void A_value_with_the_owned_bit_set_is_not_applied()
    {
        Set("510");
        Assert.Equal(TweakState.NotApplied, _engine.Detect(Make(Shortcut("506"))));
    }

    [Fact]
    public void Applying_changes_only_the_owned_bit()
    {
        Set("511");
        Assert.True(_engine.Apply(Make(Shortcut("506")), Guid.NewGuid()).Success);
        Assert.Equal("507", _reg.Read(Flags).Data!.Value);
    }

    [Fact]
    public void Applying_to_a_value_that_already_matches_writes_nothing()
    {
        Set("498");
        var writes = 0;
        _reg.FailWrite = _ => { writes++; return false; };

        Assert.True(_engine.Apply(Make(Shortcut("506")), Guid.NewGuid()).Success);

        Assert.Equal(0, writes);
        Assert.Equal("498", _reg.Read(Flags).Data!.Value);
    }

    [Fact]
    public void Revert_puts_back_the_whole_value_that_was_there()
    {
        Set("511");
        var tweak = Make(Shortcut("506"));

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.Equal("511", _reg.Read(Flags).Data!.Value);
    }

    [Fact]
    public void The_default_undo_turns_the_bit_back_on_and_keeps_the_rest()
    {
        Set("498");
        Assert.True(_engine.Revert(Make(Shortcut("506")), Guid.NewGuid()).Success);
        Assert.Equal("502", _reg.Read(Flags).Data!.Value);
    }

    [Fact]
    public void A_missing_value_is_written_whole()
    {
        Assert.True(_engine.Apply(Make(Shortcut("506")), Guid.NewGuid()).Success);
        Assert.Equal("506", _reg.Read(Flags).Data!.Value);
    }

    [Fact]
    public void A_step_recorded_without_a_mask_is_checked_with_the_catalog_mask()
    {
        // History written before the catalog knew which bit it owns holds the whole value. The check must not call 498 changed back.
        var tweak = Make(Shortcut("506"));
        var old = new SetRegistryValue(Flags, new RegistryData(RegistryKind.String, "506"));
        _journal.Append(new JournalEntry(Guid.NewGuid(), tweak.Id, JournalAction.Apply, DateTimeOffset.UtcNow, old, new SetRegistryValue(Flags, new RegistryData(RegistryKind.String, "510"))));

        Set("498");
        Assert.Empty(_engine.CheckRecorded(tweak).Broken);

        Set("510");
        Assert.Single(_engine.CheckRecorded(tweak).Broken);
    }

    [Fact]
    public void A_masked_step_is_described_by_the_bits_it_changes()
    {
        Assert.Equal("Turn bit 0x4 off in HKCU\\Control Panel\\Accessibility\\StickyKeys\\Flags (String)", Shortcut("506").Describe());
        Assert.Equal("Turn bit 0x4 on in HKCU\\Control Panel\\Accessibility\\StickyKeys\\Flags (String)", Shortcut("510").Describe());
    }

    [Fact]
    public void The_mask_survives_the_history_file()
    {
        var back = System.Text.Json.JsonSerializer.Deserialize<Operation>(System.Text.Json.JsonSerializer.Serialize<Operation>(Shortcut("506")));
        Assert.Equal(Shortcut("506"), back);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("0")]
    [InlineData("-4")]
    [InlineData("")]
    public void A_mask_that_is_not_a_plain_number_above_zero_is_rejected(string mask)
    {
        var tweak = Make(new SetRegistryValue(Flags, new RegistryData(RegistryKind.String, "506"), mask));
        Assert.Contains(CatalogLoader.Validate([tweak]), e => e.Contains("bit mask", StringComparison.Ordinal));
    }

    [Fact]
    public void A_mask_on_a_binary_value_is_rejected()
    {
        var tweak = Make(new SetRegistryValue(Flags, new RegistryData(RegistryKind.Binary, "0A"), "4"));
        Assert.Contains(CatalogLoader.Validate([tweak]), e => e.Contains("String, DWord or QWord", StringComparison.Ordinal));
    }

    [Fact]
    public void The_sticky_keys_setting_owns_only_the_shortcut_bit()
    {
        var tweak = CatalogLoader.LoadEmbedded().Single(t => t.Id == "input.sticky-keys-prompt-off");
        Assert.All(tweak.Apply.Concat(tweak.Undo), op => Assert.Equal("4", Assert.IsType<SetRegistryValue>(op).Mask));
    }
}
