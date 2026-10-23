using Clarion.Core.Actions;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class RepairWingetTests
{
    private sealed class FakeTool(int exitCode) : IStreamingRunner
    {
        public List<(string Tool, string Args)> Ran { get; } = [];

        public Task<int> RunAsync(string systemTool, string args, bool utf16, Action<string> onLine, CancellationToken cancel)
        {
            Ran.Add((systemTool, args));
            onLine("output");
            return Task.FromResult(exitCode);
        }
    }

    private static ActionDef Job() => CatalogLoader.LoadActions().Single(a => a.Id == "repair.winget");

    [Fact]
    public void The_job_is_in_the_catalog_valid_safe_and_one_typed_step()
    {
        var job = Job();

        Assert.Empty(CatalogLoader.ValidateActions(CatalogLoader.LoadActions()));
        Assert.Equal(RiskLevel.Safe, job.RiskLevel);
        Assert.IsType<RegisterAppInstaller>(Assert.Single(job.Steps));
        Assert.Contains(job.Sources, s => s.StartsWith("https://learn.microsoft.com/", StringComparison.Ordinal));
        Assert.Contains("Downloads nothing", job.Facts);
        Assert.False(job.NeedsReboot);
    }

    [Fact]
    public void The_command_is_the_one_microsoft_documents_and_names_only_app_installer()
    {
        var script = AppInstallerRegistration.Script;

        Assert.Contains("Add-AppxPackage -RegisterByFamilyName -MainPackage 'Microsoft.DesktopAppInstaller_8wekyb3d8bbwe'", script, StringComparison.Ordinal);
        Assert.Equal("Microsoft.DesktopAppInstaller_8wekyb3d8bbwe", AppInstallerRegistration.FamilyName);
    }

    [Theory]
    [InlineData("http")]
    [InlineData("Invoke-WebRequest")]
    [InlineData("Invoke-RestMethod")]
    [InlineData("Install-Module")]
    [InlineData("Install-PackageProvider")]
    [InlineData("Start-BitsTransfer")]
    [InlineData("DownloadFile")]
    [InlineData("Remove-AppxPackage")]
    [InlineData("-Uninstall")]
    public void The_fixed_script_cannot_download_or_remove_anything(string forbidden) =>
        Assert.DoesNotContain(forbidden, AppInstallerRegistration.Script, StringComparison.OrdinalIgnoreCase);

    [Fact]
    public void Progress_is_silenced_so_the_log_holds_no_raw_markup() =>
        Assert.StartsWith("$ProgressPreference = 'SilentlyContinue'; ", AppInstallerRegistration.Script, StringComparison.Ordinal);

    [Fact]
    public void The_tool_is_a_path_below_system32_and_not_something_that_leaves_it()
    {
        var tool = AppInstallerRegistration.Tool;

        Assert.False(Path.IsPathRooted(tool));
        Assert.DoesNotContain("..", tool, StringComparison.Ordinal);
        Assert.EndsWith("powershell.exe", tool, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_runner_starts_windows_powershell_with_exactly_the_fixed_script()
    {
        var fake = new FakeTool(0);

        var result = await new ActionRunner(fake).RunAsync(Job(), _ => { }, default);

        Assert.True(result.Success);
        var (tool, args) = Assert.Single(fake.Ran);
        Assert.Equal(AppInstallerRegistration.Tool, tool);
        Assert.Equal("$ErrorActionPreference='Stop'; " + AppInstallerRegistration.Script, PowerShellHost.Decode(args));
        Assert.StartsWith("-NoProfile -NonInteractive ", args, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_package_that_is_not_on_the_pc_is_said_in_plain_words_and_is_not_a_crash()
    {
        var result = await new ActionRunner(new FakeTool(2)).RunAsync(Job(), _ => { }, default);

        Assert.False(result.Success);
        Assert.Contains("not on this PC", result.Error, StringComparison.Ordinal);
        Assert.Contains("Microsoft Store", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Windows_refusing_the_registration_points_at_the_log()
    {
        var result = await new ActionRunner(new FakeTool(1)).RunAsync(Job(), _ => { }, default);

        Assert.False(result.Success);
        Assert.Contains("Read the log", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public async Task When_the_signed_in_account_is_another_one_nothing_runs_and_the_reason_is_given()
    {
        var fake = new FakeTool(0);

        var result = await new ActionRunner(fake, null, null, () => "Start it again from your own account.").RunAsync(Job(), _ => { }, default);

        Assert.False(result.Success);
        Assert.Equal("Start it again from your own account.", result.Error);
        Assert.Empty(fake.Ran);
    }

    [Fact]
    public void The_step_survives_the_catalog_file_and_describes_itself()
    {
        var step = new RegisterAppInstaller();
        Assert.Equal("Register App Installer for this account", step.Describe());
    }
}
