using Clarion.Core.Reports;

namespace Clarion.Tests;

public sealed class RestartNoticeTests
{
    [Fact]
    public void With_nothing_applied_it_does_not_say_the_zero_settings()
    {
        var text = RestartNotice.Message(["Windows servicing is waiting for a restart"], 0);
        Assert.DoesNotContain("0 setting", text);
        Assert.StartsWith("Windows servicing is waiting for a restart. ", text);
    }

    [Theory]
    [InlineData(1, "the 1 setting it applied")]
    [InlineData(5, "the 5 settings it applied")]
    public void The_count_is_singular_or_plural(int count, string expected) =>
        Assert.Contains(expected, RestartNotice.Message(["A restart is pending"], count));

    [Fact]
    public void Verify_adds_that_a_new_scan_is_needed_and_Home_does_not()
    {
        Assert.EndsWith("Scan again then.", RestartNotice.Message(["x"], 2, scanAgain: true));
        Assert.DoesNotContain("Scan again", RestartNotice.Message(["x"], 2));
    }
}
