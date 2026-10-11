using Clarion.Core.Apps;

namespace Clarion.Tests;

public sealed class WingetOutcomeTests
{
    [Fact]
    public void A_failed_installer_reports_its_own_exit_code()
    {
        var outcome = new WingetOutcome(unchecked((int)0x8A150108), "Starting package install...\r\nInstaller failed with exit code: 1603\r\n");
        Assert.Contains("exit code 1603", outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Output_without_that_line_adds_nothing()
    {
        var outcome = new WingetOutcome(unchecked((int)0x8A150108), "something else");
        Assert.Equal(WingetCodes.Explain(unchecked((int)0x8A150108)), outcome.Message);
    }

    [Fact]
    public void A_success_never_adds_a_code() =>
        Assert.Equal("Done.", new WingetOutcome(0, "Installer failed with exit code: 5").Message);

    [Fact]
    public void A_huge_number_is_not_picked_up() =>
        Assert.DoesNotContain("exit code", new WingetOutcome(unchecked((int)0x8A150108), "Installer failed with exit code: 12345678901234").Message, StringComparison.Ordinal);
}
