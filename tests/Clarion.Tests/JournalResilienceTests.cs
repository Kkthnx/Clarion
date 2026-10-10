using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class JournalResilienceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-jr-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static JournalEntry Entry(string id) => new(Guid.NewGuid(), id, JournalAction.Apply, DateTimeOffset.UtcNow,
        new SetRegistryValue(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", "V"), new RegistryData(RegistryKind.DWord, "1")),
        new DeleteRegistryValue(new RegistryTarget(RegistryHive.CurrentUser, "Software\\T", "V"), "Software"));

    [Fact]
    public void A_damaged_line_is_skipped_and_the_rest_is_kept()
    {
        var path = Path.Combine(_dir, "j.jsonl");
        var journal = new ChangeJournal(path);
        journal.Append(Entry("a"));
        File.AppendAllText(path, "{ this is not json\n");
        journal.Append(Entry("b"));

        var all = journal.ReadAll();

        Assert.Equal(["a", "b"], all.Select(e => e.TweakId).ToArray());
        Assert.Equal(1, journal.SkippedLines);
    }

    [Fact]
    public void A_cut_off_last_line_does_not_swallow_the_next_entry()
    {
        var path = Path.Combine(_dir, "j.jsonl");
        var journal = new ChangeJournal(path);
        journal.Append(Entry("a"));
        File.AppendAllText(path, "{\"BatchId\":\"abc");
        journal.Append(Entry("b"));

        var all = journal.ReadAll();

        Assert.Contains(all, e => e.TweakId == "a");
        Assert.Contains(all, e => e.TweakId == "b");
    }

    [Fact]
    public void Reading_while_another_process_is_writing_does_not_fail()
    {
        // The monthly check runs as its own process and reads the journal while the window may be appending to it.
        var path = Path.Combine(_dir, "j.jsonl");
        var journal = new ChangeJournal(path);
        journal.Append(Entry("a"));

        using var writer = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        var all = new ChangeJournal(path).ReadAll();

        Assert.Equal(["a"], all.Select(e => e.TweakId).ToArray());
    }

    [Fact]
    public void Appending_waits_out_another_writer_that_is_about_to_finish()
    {
        var path = Path.Combine(_dir, "j.jsonl");
        var journal = new ChangeJournal(path);
        journal.Append(Entry("a"));

        var other = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        // A dedicated thread, not the thread pool: other tests block pool threads, and on a small build machine the pool can stay
        // starved for longer than Append is willing to wait.
        var release = new Thread(() => { Thread.Sleep(150); other.Dispose(); }) { IsBackground = true };
        release.Start();

        journal.Append(Entry("b"));
        release.Join();

        Assert.Equal(["a", "b"], new ChangeJournal(path).ReadAll().Select(e => e.TweakId).ToArray());
    }

    [Fact]
    public void An_empty_or_missing_journal_reads_as_empty()
    {
        var journal = new ChangeJournal(Path.Combine(_dir, "none.jsonl"));
        Assert.Empty(journal.ReadAll());
        Assert.Empty(journal.OutstandingFor("x"));
    }
}
