using Clarion.Core.Model;
using Clarion.Core.SystemInfo;

namespace Clarion.Tests;

public sealed class UpdateStateTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 10, 12, 0, 0, TimeSpan.Zero);

    private static UpdateInputs Healthy() => new()
    {
        LastCheck = Now.AddDays(-1),
        LastInstall = Now.AddDays(-9),
    };

    [Fact]
    public void A_normal_PC_with_a_recent_install_is_working_and_says_why()
    {
        var v = UpdateState.Evaluate(Healthy(), Now);
        Assert.Equal(UpdateLevel.Working, v.Level);
        Assert.All(v.Findings, f => Assert.Equal(UpdateFindingKind.Good, f.Kind));
        Assert.Equal(2, v.Findings.Count);
        Assert.Equal("Windows Update is working", v.Headline);
    }

    [Fact]
    public void A_stored_pause_to_2077_that_windows_reports_as_ended_is_only_a_note()
    {
        // As read from a real Ghost Spectre PC: pause dates to 2077, both status values 2, and updates installing.
        var v = UpdateState.Evaluate(Healthy() with
        {
            PauseEnds = new DateTimeOffset(2077, 1, 1, 10, 38, 56, TimeSpan.Zero),
            PausedFeatureStatus = 2,
            PausedQualityStatus = 2,
        }, Now);

        Assert.Equal(UpdateLevel.Working, v.Level);
        var note = Assert.Single(v.Findings, f => f.Kind == UpdateFindingKind.Info);
        Assert.Contains("reports it as ended", note.Text);
        Assert.Contains("35 days", note.Text);
        Assert.Contains("notes below are worth knowing", v.Explanation);
    }

    [Fact]
    public void A_long_pause_that_windows_still_counts_is_held()
    {
        var v = UpdateState.Evaluate(Healthy() with
        {
            PauseEnds = new DateTimeOffset(2077, 1, 1, 0, 0, 0, TimeSpan.Zero),
            PausedFeatureStatus = 1,
            PausedQualityStatus = 1,
        }, Now);
        Assert.Equal(UpdateLevel.Held, v.Level);
        Assert.Contains(v.Findings, f => f.Kind == UpdateFindingKind.Problem && f.Text.Contains("Updates are paused until"));
    }

    [Fact]
    public void A_pause_with_no_status_to_say_otherwise_counts_as_in_effect()
    {
        var v = UpdateState.Evaluate(new UpdateInputs { PauseEnds = Now.AddYears(40) }, Now);
        Assert.Equal(UpdateLevel.Held, v.Level);
    }

    [Fact]
    public void An_ordinary_pause_is_a_limit_not_a_fault()
    {
        var v = UpdateState.Evaluate(Healthy() with { PauseEnds = Now.AddDays(10), PausedFeatureStatus = 1, PausedQualityStatus = 1 }, Now);
        Assert.Equal(UpdateLevel.Limited, v.Level);
        Assert.Contains(v.Findings, f => f.Kind == UpdateFindingKind.Limit && f.Text.Contains("paused until"));
    }

    [Fact]
    public void A_pause_that_has_passed_is_not_mentioned()
    {
        var v = UpdateState.Evaluate(Healthy() with { PauseEnds = Now.AddDays(-3) }, Now);
        Assert.DoesNotContain(v.Findings, f => f.Text.Contains("paused"));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void A_missing_or_disabled_update_service_holds_updates_back(bool missing, bool disabled)
    {
        var v = UpdateState.Evaluate(Healthy() with { WindowsUpdateServiceMissing = missing, WindowsUpdateServiceDisabled = disabled }, Now);
        Assert.Equal(UpdateLevel.Held, v.Level);
    }

    [Fact]
    public void A_missing_orchestrator_holds_updates_back_but_a_missing_medic_does_not()
    {
        Assert.Equal(UpdateLevel.Held, UpdateState.Evaluate(Healthy() with { OrchestratorMissing = true }, Now).Level);

        var medic = UpdateState.Evaluate(Healthy() with { MedicMissing = true }, Now);
        Assert.Equal(UpdateLevel.Working, medic.Level);
        Assert.Contains(medic.Findings, f => f.Kind == UpdateFindingKind.Info && f.Text.Contains("Medic"));
    }

    [Fact]
    public void Tiny11_Core_style_policies_hold_updates_back()
    {
        var v = UpdateState.Evaluate(new UpdateInputs
        {
            WindowsUpdateServiceDisabled = true, OrchestratorMissing = true, MedicMissing = true,
            AccessBlockedByPolicy = true, NoConnectPolicy = true, AutoUpdateOff = true,
            UseUpdateServer = true, UpdateServer = "localhost", SettingsPageHidden = true,
        }, Now);

        Assert.Equal(UpdateLevel.Held, v.Level);
        Assert.Contains(v.Findings, f => f.Kind == UpdateFindingKind.Problem && f.Text.Contains("localhost"));
        Assert.Contains(v.Findings, f => f.Text.Contains("blocks access"));
        Assert.Contains(v.Findings, f => f.Text.Contains("hidden in Settings"));
    }

    [Theory]
    [InlineData("localhost", true)]
    [InlineData("http://localhost:8530", true)]
    [InlineData("https://127.0.0.1", true)]
    [InlineData("::1", true)]
    [InlineData("http://wsus.example.com:8530", false)]
    public void The_update_server_check_tells_this_PC_from_a_company_server(string server, bool thisPc)
    {
        Assert.Equal(thisPc, UpdateState.IsThisPc(server));
        var v = UpdateState.Evaluate(Healthy() with { UseUpdateServer = true, UpdateServer = server }, Now);
        Assert.Equal(thisPc ? UpdateLevel.Held : UpdateLevel.Limited, v.Level);
    }

    [Fact]
    public void A_server_that_is_set_but_not_switched_on_is_ignored()
    {
        var v = UpdateState.Evaluate(Healthy() with { UseUpdateServer = false, UpdateServer = "localhost" }, Now);
        Assert.Equal(UpdateLevel.Working, v.Level);
    }

    [Fact]
    public void Automatic_updates_off_or_no_background_scan_limits_but_does_not_hold()
    {
        Assert.Equal(UpdateLevel.Limited, UpdateState.Evaluate(Healthy() with { AutoUpdateOff = true }, Now).Level);
        Assert.Equal(UpdateLevel.Limited, UpdateState.Evaluate(Healthy() with { ScanTaskEnabled = false }, Now).Level);
        Assert.Equal(UpdateLevel.Working, UpdateState.Evaluate(Healthy() with { ScanTaskEnabled = true }, Now).Level);
    }

    [Fact]
    public void Old_dates_are_not_offered_as_proof_it_works()
    {
        var v = UpdateState.Evaluate(new UpdateInputs { LastCheck = Now.AddDays(-30), LastInstall = Now.AddDays(-200) }, Now);
        Assert.Empty(v.Findings);
        Assert.Equal(UpdateLevel.Working, v.Level);
    }
}
