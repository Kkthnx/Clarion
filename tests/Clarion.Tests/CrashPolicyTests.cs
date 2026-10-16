using Clarion.Core.Diagnostics;

namespace Clarion.Tests;

public sealed class CrashPolicyTests
{
    private sealed class FakeClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }

    [Fact]
    public void An_error_while_idle_resets_to_a_known_state_and_one_during_a_run_leaves_the_run_to_finish()
    {
        var policy = new CrashPolicy(new FakeClock());

        Assert.Equal(CrashAction.SafeState, policy.Next(changingThisPc: false));
        Assert.Equal(CrashAction.Continue, policy.Next(changingThisPc: true));
    }

    [Fact]
    public void A_run_of_errors_ends_the_program_instead_of_resetting_forever()
    {
        var policy = new CrashPolicy(new FakeClock(), limit: 3);

        for (var i = 0; i < 3; i++) Assert.NotEqual(CrashAction.Terminate, policy.Next(false));
        Assert.Equal(CrashAction.Terminate, policy.Next(false));
        // Even during a run: errors that keep coming mean the state cannot be trusted.
        Assert.Equal(CrashAction.Terminate, policy.Next(true));
    }

    [Fact]
    public void Errors_that_are_far_apart_never_add_up()
    {
        var clock = new FakeClock();
        var policy = new CrashPolicy(clock, limit: 2, window: TimeSpan.FromMinutes(1));

        for (var i = 0; i < 20; i++)
        {
            Assert.Equal(CrashAction.SafeState, policy.Next(false));
            clock.Now += TimeSpan.FromMinutes(2);
        }
    }
}
