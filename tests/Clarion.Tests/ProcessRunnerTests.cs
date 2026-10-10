using System.Diagnostics;
using System.Runtime.Versioning;
using Clarion.Engine;

namespace Clarion.Tests;

[SupportedOSPlatform("windows")]
public sealed class ProcessRunnerTests
{
    [Fact]
    public void Output_of_a_finished_program_is_returned()
    {
        var result = new WindowsProcessRunner().Run("cmd.exe", "/c echo hello", TimeSpan.FromSeconds(10));

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("hello", result.Output);
    }

    [Fact]
    public void A_program_that_runs_too_long_is_stopped_and_reported()
    {
        var watch = Stopwatch.StartNew();
        var result = new WindowsProcessRunner().Run("ping.exe", "-n 30 127.0.0.1", TimeSpan.FromSeconds(1));

        Assert.Equal(-1, result.ExitCode);
        Assert.Contains("timed out", result.Error);
        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(10), $"took {watch.Elapsed}");
    }

    [Fact]
    public void A_program_that_leaves_a_helper_running_does_not_hold_the_caller_past_the_timeout()
    {
        // The helper keeps the output pipes open after cmd.exe has exited, which used to block the read until the helper ended.
        var watch = Stopwatch.StartNew();
        var result = new WindowsProcessRunner().Run("cmd.exe", "/c start /b ping.exe -n 25 127.0.0.1", TimeSpan.FromSeconds(3));

        Assert.True(watch.Elapsed < TimeSpan.FromSeconds(10), $"took {watch.Elapsed}");
        Assert.Equal(0, result.ExitCode);
    }
}
