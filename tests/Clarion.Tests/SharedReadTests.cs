using Clarion.Core.Engine;

namespace Clarion.Tests;

public sealed class SharedReadTests
{
    [Fact]
    public async Task Callers_that_ask_together_share_one_run()
    {
        var runs = 0;
        using var release = new ManualResetEventSlim();
        var shared = new SharedRead<string>(() =>
        {
            Interlocked.Increment(ref runs);
            release.Wait(TimeSpan.FromSeconds(10));
            return "report";
        });

        var first = shared.GetAsync();
        var second = shared.GetAsync();
        release.Set();

        Assert.Equal("report", await first);
        Assert.Equal("report", await second);
        Assert.Equal(1, runs);
    }

    [Fact]
    public async Task A_finished_read_is_reused_until_a_refresh_is_asked_for()
    {
        var runs = 0;
        var shared = new SharedRead<string>(() => "run " + Interlocked.Increment(ref runs));

        Assert.Equal("run 1", await shared.GetAsync());
        Assert.Equal("run 1", await shared.GetAsync());
        Assert.Equal("run 2", await shared.GetAsync(refresh: true));
        Assert.Equal("run 2", shared.Latest);
    }

    [Fact]
    public async Task A_failed_read_is_tried_again_by_the_next_caller()
    {
        var runs = 0;
        var shared = new SharedRead<string>(() =>
        {
            if (Interlocked.Increment(ref runs) == 1) throw new InvalidOperationException("probe failed");
            return "ok";
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => shared.GetAsync());
        Assert.Null(shared.Latest);
        Assert.Equal("ok", await shared.GetAsync());
        Assert.Equal(2, runs);
    }

    [Fact]
    public async Task A_slow_older_read_does_not_replace_a_newer_refresh()
    {
        using var releaseOld = new ManualResetEventSlim();
        using var oldStarted = new ManualResetEventSlim();
        var runs = 0;
        var shared = new SharedRead<string>(() =>
        {
            if (Interlocked.Increment(ref runs) == 1)
            {
                oldStarted.Set();
                releaseOld.Wait(TimeSpan.FromSeconds(10));
                return "old";
            }
            return "new";
        });

        var old = shared.GetAsync();
        // The thread pool may start either read first, so the refresh waits until the older one is really running.
        Assert.True(oldStarted.Wait(TimeSpan.FromSeconds(10)));
        Assert.Equal("new", await shared.GetAsync(refresh: true));
        releaseOld.Set();
        Assert.Equal("old", await old);

        Assert.Equal("new", shared.Latest);
    }
}
