using Clarion.Core.SystemInfo;
using Clarion.Engine;

namespace Clarion.Tests;

public sealed class SafeModeTests
{
    [Theory]
    [InlineData(0, BootMode.Normal)]
    [InlineData(1, BootMode.SafeMode)]
    [InlineData(2, BootMode.SafeModeWithNetwork)]
    public void The_system_metric_maps_to_how_windows_started(int metric, BootMode expected) =>
        Assert.Equal(expected, SafeModeNotice.FromMetric(metric));

    [Fact]
    public void A_normal_start_has_nothing_to_say() =>
        Assert.Equal("", SafeModeNotice.Message(BootMode.Normal));

    [Theory]
    [InlineData(BootMode.SafeMode)]
    [InlineData(BootMode.SafeModeWithNetwork)]
    public void Safe_mode_says_what_stops_working_and_how_to_get_it_back(BootMode mode)
    {
        var text = SafeModeNotice.Message(mode);
        Assert.Contains("apps", text, StringComparison.Ordinal);
        Assert.Contains("scheduled tasks", text, StringComparison.Ordinal);
        Assert.Contains("registry", text, StringComparison.Ordinal);
        Assert.Contains("Restart Windows normally", text, StringComparison.Ordinal);
    }

    [Fact]
    public void This_test_run_is_a_normal_start()
    {
        if (!OperatingSystem.IsWindows()) return;
        // The tests are run from a normal session. A run from Safe Mode would be told apart here.
        Assert.Equal(BootMode.Normal, WindowsBootMode.Detect());
    }
}
