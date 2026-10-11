using System.Runtime.Versioning;
using Clarion.Core.Apps;
using Clarion.Engine;

namespace Clarion.Tests;

/// <summary>
/// Runs the read-only winget commands against the real machine. Nothing is installed or removed.
/// Skipped unless CLARION_REAL_TESTS=1 and winget is there.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RealWingetTests
{
    private static bool Enabled =>
        OperatingSystem.IsWindows() && Environment.GetEnvironmentVariable("CLARION_REAL_TESTS") == "1" && File.Exists(WindowsWingetStore.DefaultPath);

    [Fact]
    public void Show_reads_a_package_record_with_a_checksum_and_its_installer_host()
    {
        if (!Enabled) return;
        var info = new WindowsWingetStore(new WindowsProcessRunner()).Show("7zip.7zip");

        Assert.NotNull(info);
        Assert.Equal("7zip.7zip", info!.Id);
        Assert.Matches("^[0-9a-fA-F]{64}$", info.Sha256);
        Assert.NotEmpty(info.InstallerHost);
        Assert.NotEmpty(info.Version);
    }

    [Fact]
    public void Show_keeps_a_registered_sign_in_a_name()
    {
        if (!Enabled) return;
        // winget writes UTF-8 to a pipe. Read with the wrong code page, the sign would come back as two odd characters.
        var info = new WindowsWingetStore(new WindowsProcessRunner()).Show("REALiX.HWiNFO");

        Assert.NotNull(info);
        Assert.Contains("®", info!.Name, StringComparison.Ordinal);
        Assert.DoesNotContain("Â", info.Name, StringComparison.Ordinal);
    }

    [Fact]
    public void The_installed_list_can_be_read_and_every_id_is_valid()
    {
        if (!Enabled) return;
        var ids = new WindowsWingetStore(new WindowsProcessRunner()).InstalledIds();

        Assert.NotNull(ids);
        Assert.All(ids, id => Assert.True(WingetIds.IsValid(id), id));
    }

    [Fact]
    public void An_id_that_does_not_exist_gives_no_record()
    {
        if (!Enabled) return;
        Assert.Null(new WindowsWingetStore(new WindowsProcessRunner()).Show("Clarion.DoesNotExist"));
    }
}
