using Clarion.Core.Profiles;

namespace Clarion.Tests;

public sealed class QueueCarryTests : IDisposable
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-carry-" + Guid.NewGuid().ToString("N"));
    private string File1 => Path.Combine(_dir, "carried-queue.json");

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch (IOException) { }
    }

    [Fact]
    public void A_queue_comes_back_once()
    {
        var carry = new QueueCarry(File1);
        Assert.True(carry.Save(["privacy.a", "app.putty", "privacy.a"]));

        Assert.Equal(["app.putty", "privacy.a"], carry.Take().Order(StringComparer.Ordinal));
        Assert.Empty(carry.Take());
        Assert.False(File.Exists(File1));
    }

    [Fact]
    public void An_empty_queue_writes_nothing_and_clears_an_older_file()
    {
        var carry = new QueueCarry(File1);
        carry.Save(["privacy.a"]);
        Assert.False(carry.Save([]));
        Assert.False(File.Exists(File1));
    }

    [Fact]
    public void A_queue_older_than_ten_minutes_is_ignored_and_removed()
    {
        var clock = new FixedClock(DateTimeOffset.UtcNow);
        var carry = new QueueCarry(File1, clock);
        carry.Save(["privacy.a"]);

        clock.Now = DateTimeOffset.UtcNow + QueueCarry.MaxAge + TimeSpan.FromMinutes(1);

        Assert.Empty(carry.Take());
        Assert.False(File.Exists(File1));
    }

    [Fact]
    public void A_file_that_is_not_a_setup_is_ignored_and_removed()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(File1, "{ \"not\": \"a setup\" }");
        Assert.Empty(new QueueCarry(File1).Take());
        Assert.False(File.Exists(File1));
    }

    [Fact]
    public void A_name_that_could_not_be_a_setting_is_refused_with_the_whole_file()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(File1, ProfileFile.Serialize(["privacy.a"], "x").Replace("privacy.a", "..\\evil path", StringComparison.Ordinal));
        Assert.Empty(new QueueCarry(File1).Take());
    }

    [Fact]
    public void A_missing_file_gives_nothing()
    {
        Assert.Empty(new QueueCarry(File1).Take());
    }

    [Fact]
    public void Discard_removes_what_was_saved()
    {
        var carry = new QueueCarry(File1);
        carry.Save(["privacy.a"]);
        carry.Discard();
        Assert.False(File.Exists(File1));
    }
}
