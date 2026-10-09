using Clarion.Core.Cleanup;

namespace Clarion.Tests;

public sealed class FakePendingRenameStore : IPendingRenameStore
{
    public List<string> Entries { get; } = [];
    public int Writes { get; private set; }

    public IReadOnlyList<string> Read() => Entries.ToList();

    public void Write(IReadOnlyList<string> entries)
    {
        Writes++;
        Entries.Clear();
        Entries.AddRange(entries);
    }

    /// <summary>What MoveFileEx writes for a delete: the NT path, then an empty entry.</summary>
    public void QueueDelete(string path, string prefix = "")
    {
        Entries.Add(prefix + "\\??\\" + path);
        Entries.Add("");
    }
}

public sealed class RestartQueueTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-queue-" + Guid.NewGuid().ToString("N"));
    private readonly FakePendingRenameStore _store = new();
    private readonly RestartQueue _queue;

    public RestartQueueTests()
    {
        _queue = new RestartQueue(Path.Combine(_dir, "restart-queue.json"), _store);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    [Theory]
    [InlineData("\\??\\C:\\Temp\\a.tmp", "C:\\Temp\\a.tmp")]
    [InlineData("*1\\??\\C:\\Temp\\a.tmp", "C:\\Temp\\a.tmp")]
    [InlineData("C:\\Temp\\a.tmp", "C:\\Temp\\a.tmp")]
    public void Entries_are_read_without_the_windows_prefixes(string raw, string expected) =>
        Assert.Equal(expected, PendingDeletes.Normalize(raw));

    [Fact]
    public void Only_delete_operations_are_listed_and_renames_are_not()
    {
        var raw = new List<string> { "\\??\\C:\\a", "", "\\??\\C:\\b", "\\??\\C:\\b.new", "\\??\\C:\\c", "" };

        Assert.Equal(["C:\\a", "C:\\c"], PendingDeletes.DeleteTargets(raw));
        Assert.Equal(3, PendingDeletes.OperationCount(raw));
    }

    [Fact]
    public void Records_add_up_across_runs_and_are_not_duplicated()
    {
        _queue.Record(["C:\\one", "C:\\two"]);
        _queue.Record(["C:\\two", "C:\\three"]);
        foreach (var f in new[] { "C:\\one", "C:\\two", "C:\\three" }) _store.QueueDelete(f);

        Assert.Equal(["C:\\one", "C:\\two", "C:\\three"], _queue.View().Waiting.Select(w => w.Path));
    }

    [Fact]
    public void Other_programs_queued_work_is_counted_but_never_listed_as_ours()
    {
        _store.QueueDelete("C:\\Temp\\installer.tmp", prefix: "*1");
        _store.QueueDelete("C:\\ours.bin");
        _queue.Record(["C:\\ours.bin"]);

        var view = _queue.View();

        Assert.Equal("C:\\ours.bin", Assert.Single(view.Waiting).Path);
        Assert.Equal(1, view.OtherOperations);
    }

    [Fact]
    public void A_file_windows_no_longer_has_queued_moves_to_the_no_longer_queued_list()
    {
        _queue.Record(["C:\\ours.bin"]);

        var view = _queue.View();

        Assert.Empty(view.Waiting);
        Assert.Equal("C:\\ours.bin", Assert.Single(view.NoLongerQueued).Path);
    }

    [Fact]
    public void Cancel_removes_only_clarions_files_and_keeps_everything_else_in_order()
    {
        _store.QueueDelete("C:\\Temp\\first.tmp", prefix: "*1");
        _store.QueueDelete("C:\\ours.bin");
        _store.Entries.Add("\\??\\C:\\old.dll");
        _store.Entries.Add("\\??\\C:\\old.dll.new");
        _store.QueueDelete("C:\\Temp\\last.tmp");
        _queue.Record(["C:\\ours.bin"]);

        var removed = _queue.Cancel();

        Assert.Equal(1, removed);
        Assert.Equal(
            ["*1\\??\\C:\\Temp\\first.tmp", "", "\\??\\C:\\old.dll", "\\??\\C:\\old.dll.new", "\\??\\C:\\Temp\\last.tmp", ""],
            _store.Entries);
        Assert.Empty(_queue.View().Waiting);
    }

    [Fact]
    public void Cancel_does_not_remove_a_rename_even_if_it_names_one_of_our_files()
    {
        _store.Entries.Add("\\??\\C:\\ours.bin");
        _store.Entries.Add("\\??\\C:\\moved.bin");
        _queue.Record(["C:\\ours.bin"]);

        Assert.Equal(0, _queue.Cancel());
        Assert.Equal(2, _store.Entries.Count);
        Assert.Equal(0, _store.Writes);
    }

    [Fact]
    public void Cancel_with_nothing_queued_writes_nothing()
    {
        Assert.Equal(0, _queue.Cancel());
        Assert.Equal(0, _store.Writes);
    }

    [Fact]
    public void Cancel_leaves_an_empty_list_for_windows_when_only_our_files_were_in_it()
    {
        _store.QueueDelete("C:\\ours.bin");
        _queue.Record(["C:\\ours.bin"]);

        _queue.Cancel();

        Assert.Empty(_store.Entries);
        Assert.Equal(1, _store.Writes);
    }

    [Fact]
    public void After_a_restart_the_record_is_settled_and_says_how_many_files_remain()
    {
        _queue.Record(["C:\\gone.bin", "C:\\stuck.bin"]);
        var afterBoot = DateTime.UtcNow.AddMinutes(5);

        var (processed, stillThere) = _queue.Settle(afterBoot, path => path.EndsWith("stuck.bin", StringComparison.Ordinal));

        Assert.Equal(2, processed);
        Assert.Equal(1, stillThere);
        Assert.Empty(_queue.View().NoLongerQueued);
    }

    [Fact]
    public void Files_queued_after_the_last_boot_are_not_settled_early()
    {
        _queue.Record(["C:\\new.bin"]);
        var lastBoot = DateTime.UtcNow.AddHours(-3);

        Assert.Equal((0, 0), _queue.Settle(lastBoot, _ => true));
    }

    [Fact]
    public void A_damaged_record_file_is_treated_as_empty()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "restart-queue.json"), "{ not json");

        Assert.Empty(_queue.View().Waiting);
        _queue.Record(["C:\\a"]);
        _store.QueueDelete("C:\\a");
        Assert.Single(_queue.View().Waiting);
    }
}
