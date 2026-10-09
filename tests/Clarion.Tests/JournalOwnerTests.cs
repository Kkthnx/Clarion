using Clarion.Core.Drift;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class JournalOwnerTests : IDisposable
{
    private const string Alice = "S-1-5-21-1-1-1-1001";
    private const string Bob = "S-1-5-21-1-1-1-1002";

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-owner-" + Guid.NewGuid().ToString("N"));
    private string JournalPath => Path.Combine(_dir, "journal.jsonl");
    private static readonly MachineProfile Pro = new(26100, "Professional");

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static SetRegistryValue UserValue(string name) =>
        new(new RegistryTarget(RegistryHive.CurrentUser, "Software\\Test", name), new RegistryData(RegistryKind.DWord, "0"));

    private static SetRegistryValue MachineValue(string name) =>
        new(new RegistryTarget(RegistryHive.LocalMachine, "Software\\Test", name), new RegistryData(RegistryKind.DWord, "0"));

    private static JournalEntry Entry(string tweak, Operation op, JournalAction action = JournalAction.Apply, string? sid = null) =>
        new(Guid.NewGuid(), tweak, action, DateTimeOffset.UtcNow, op, op, sid);

    [Fact]
    public void A_step_that_only_affects_one_account_is_stamped_with_that_account()
    {
        var journal = new ChangeJournal(JournalPath, Alice);
        journal.Append(Entry("u", UserValue("A")));

        Assert.Equal(Alice, Assert.Single(journal.ReadAll()).UserSid);
    }

    [Fact]
    public void A_machine_wide_step_is_not_stamped()
    {
        var journal = new ChangeJournal(JournalPath, Alice);
        journal.Append(Entry("m", MachineValue("A")));

        Assert.Null(Assert.Single(journal.ReadAll()).UserSid);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void An_app_removal_belongs_to_one_account_unless_it_removes_for_everyone(bool allUsers, bool perUser) =>
        Assert.Equal(perUser, !allUsers && ChangeJournal.IsPerUser(new RemoveAppxPackage("Contoso.App", allUsers)));

    [Fact]
    public void Another_accounts_per_account_settings_are_not_seen_as_outstanding()
    {
        new ChangeJournal(JournalPath, Alice).Append(Entry("u", UserValue("A")));

        var asBob = new ChangeJournal(JournalPath, Bob);

        Assert.Empty(asBob.OutstandingByTweak());
        Assert.Empty(asBob.TweakIdsWithOutstanding());
        Assert.Empty(asBob.OutstandingFor("u"));
        Assert.Contains("u", new ChangeJournal(JournalPath, Alice).TweakIdsWithOutstanding());
    }

    [Fact]
    public void Machine_wide_settings_are_seen_by_every_account()
    {
        new ChangeJournal(JournalPath, Alice).Append(Entry("m", MachineValue("A")));

        Assert.Contains("m", new ChangeJournal(JournalPath, Bob).TweakIdsWithOutstanding());
    }

    [Fact]
    public void Entries_written_before_owners_were_recorded_stay_visible_to_everyone()
    {
        // A journal opened with no known account writes no owner, as every earlier version did.
        new ChangeJournal(JournalPath).Append(Entry("old", UserValue("A")));
        Assert.Null(new ChangeJournal(JournalPath).ReadAll()[0].UserSid);

        Assert.Contains("old", new ChangeJournal(JournalPath, Alice).TweakIdsWithOutstanding());
        Assert.Contains("old", new ChangeJournal(JournalPath, Bob).TweakIdsWithOutstanding());
    }

    [Fact]
    public void A_journal_line_from_an_older_version_without_an_owner_still_reads()
    {
        var current = new ChangeJournal(JournalPath);
        current.Append(Entry("x", UserValue("A")));
        var line = File.ReadAllText(JournalPath).Replace(",\"UserSid\":null", "");
        Assert.DoesNotContain("UserSid", line);
        File.WriteAllText(JournalPath, line);

        Assert.Single(new ChangeJournal(JournalPath, Alice).ReadAll());
    }

    [Fact]
    public void One_accounts_revert_does_not_close_the_other_accounts_entry()
    {
        var alice = new ChangeJournal(JournalPath, Alice);
        var bob = new ChangeJournal(JournalPath, Bob);
        alice.Append(Entry("u", UserValue("A")));
        bob.Append(Entry("u", UserValue("A")));
        bob.Append(Entry("u", UserValue("A"), JournalAction.Revert));

        Assert.Contains("u", alice.TweakIdsWithOutstanding());
        Assert.DoesNotContain("u", bob.TweakIdsWithOutstanding());
    }

    [Fact]
    public void A_second_account_does_not_see_the_first_accounts_settings_as_changed_back()
    {
        var reg = new FakeRegistry();
        var tweak = new Tweak
        {
            Id = "u.one", Category = "Test", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
            Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe, Apply = [UserValue("A")],
        };

        // Alice applies. Her hive holds the value, and so does the fake store she was reading.
        var aliceJournal = new ChangeJournal(JournalPath, Alice);
        var aliceEngine = new TweakEngine([new RegistryHandler(reg)], aliceJournal);
        Assert.True(aliceEngine.Apply(tweak, Guid.NewGuid()).Success);

        // Bob opens Clarion. His own hive has no such value, which must not look like Windows undoing something.
        reg.DeleteValue(tweak.Apply.OfType<SetRegistryValue>().Single().Target);
        var bobJournal = new ChangeJournal(JournalPath, Bob);
        var bobScan = new DriftScanner(new TweakEngine([new RegistryHandler(reg)], bobJournal), bobJournal).Scan([tweak], Pro);

        Assert.False(bobScan.HasDrift);
        Assert.Equal(0, bobScan.Checked);

        // Alice's own scan still reports it.
        var aliceScan = new DriftScanner(aliceEngine, aliceJournal).Scan([tweak], Pro);
        Assert.True(aliceScan.HasDrift);
    }
}
