using Clarion.Core.Abstractions;
using Clarion.Core.Appx;
using Clarion.Core.Drift;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class DriftTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-drift-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();
    private readonly FakeAppxStore _apps = new();
    private readonly ChangeJournal _journal;
    private readonly TweakEngine _engine;
    private readonly DriftState _state;
    private readonly DriftScanner _scanner;
    private static readonly MachineProfile Pro = new(26100, "Professional");

    public DriftTests()
    {
        _journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        _engine = new TweakEngine([new RegistryHandler(_reg), new AppxHandler(_apps)], _journal);
        _state = new DriftState(Path.Combine(_dir, "drift.json"));
        _scanner = new DriftScanner(_engine, _journal, _state);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static RegistryTarget T(string name) => new(RegistryHive.CurrentUser, "Software\\Test", name);

    private static Tweak Make(string id, params Operation[] ops) => new()
    {
        Id = id, Category = "Test", Name = id, Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe, Apply = ops,
    };

    private static SetRegistryValue Dword(string name, string v) => new(T(name), new RegistryData(RegistryKind.DWord, v));

    private void WindowsSets(string name, string value) => _reg.Write(T(name), new RegistryData(RegistryKind.DWord, value));

    [Fact]
    public void Nothing_drifted_when_everything_still_holds()
    {
        var tweak = Make("a", Dword("A", "0"));
        _engine.Apply(tweak, Guid.NewGuid());

        var report = _scanner.Scan([tweak], Pro);

        Assert.False(report.HasDrift);
        Assert.Equal(1, report.Checked);
    }

    [Fact]
    public void A_value_set_back_by_windows_is_reported()
    {
        var tweak = Make("a", Dword("A", "0"));
        _engine.Apply(tweak, Guid.NewGuid());
        WindowsSets("A", "1");

        var report = _scanner.Scan([tweak], Pro);

        var item = Assert.Single(report.Items);
        Assert.Equal(TweakState.NotApplied, item.State);
        Assert.False(item.IsReturnedApp);
        Assert.Single(item.Changed);
        Assert.Single(report.ChangedBack);
        Assert.Empty(report.ReturnedApps);
    }

    [Fact]
    public void A_setting_that_only_partly_came_back_names_just_the_broken_step()
    {
        var tweak = Make("a", Dword("A", "0"), Dword("B", "0"));
        _engine.Apply(tweak, Guid.NewGuid());
        WindowsSets("B", "5");

        var item = Assert.Single(_scanner.Scan([tweak], Pro).Items);

        Assert.Equal(TweakState.Partial, item.State);
        Assert.Contains("\\B", Assert.Single(item.Changed));
    }

    [Fact]
    public void A_removed_app_that_returns_is_reported_as_a_returned_app()
    {
        _apps.Add("Contoso.Widget");
        var tweak = Make("app.widget", new RemoveAppxPackage("Contoso.Widget"));
        _engine.Apply(tweak, Guid.NewGuid());
        Assert.False(_scanner.Scan([tweak], Pro).HasDrift);

        _apps.Current.Add("Contoso.Widget");

        var report = _scanner.Scan([tweak], Pro);
        var item = Assert.Single(report.ReturnedApps);
        Assert.Equal("Contoso.Widget", Assert.Single(item.ReturnedApps));
        Assert.Empty(report.ChangedBack);
    }

    [Fact]
    public void Fixing_a_returned_app_removes_it_again_and_clears_the_report()
    {
        _apps.Add("Contoso.Widget");
        var tweak = Make("app.widget", new RemoveAppxPackage("Contoso.Widget"));
        _engine.Apply(tweak, Guid.NewGuid());
        _apps.Current.Add("Contoso.Widget");

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);

        Assert.False(_scanner.Scan([tweak], Pro).HasDrift);
        Assert.DoesNotContain("Contoso.Widget", _apps.Current);
    }

    [Fact]
    public void The_deprovision_variant_stops_new_accounts_and_needs_machine_scope()
    {
        var plain = Make("app.widget", new RemoveAppxPackage("Contoso.Widget"));
        Assert.True(DeprovisionVariant.CanApply(plain));

        var variant = DeprovisionVariant.Of(plain);

        Assert.Equal(TweakScope.Machine, variant.Scope);
        Assert.True(Assert.IsType<RemoveAppxPackage>(Assert.Single(variant.Apply)).Deprovision);
        Assert.False(DeprovisionVariant.CanApply(variant));
        Assert.False(DeprovisionVariant.CanApply(Make("a", Dword("A", "0"))));
        Assert.False(Assert.IsType<RemoveAppxPackage>(Assert.Single(plain.Apply)).Deprovision);
    }

    [Fact]
    public void An_app_that_is_provisioned_again_is_drift_even_though_it_is_not_installed()
    {
        _apps.Add("Contoso.Widget");
        var plain = Make("app.widget", new RemoveAppxPackage("Contoso.Widget"));
        Assert.True(_engine.Apply(DeprovisionVariant.Of(plain), Guid.NewGuid()).Success);
        Assert.False(_scanner.Scan([plain], Pro).HasDrift);
        Assert.True(DeprovisionVariant.WasRecorded(_journal.OutstandingFor("app.widget")));

        _apps.Provisioned.Add("Contoso.Widget");

        var report = _scanner.Scan([plain], Pro);
        var item = Assert.Single(report.Items);
        Assert.False(item.IsReturnedApp);
        Assert.Contains("new accounts", Assert.Single(item.Changed));
    }

    [Fact]
    public void A_plain_removal_does_not_care_about_provisioning()
    {
        _apps.Add("Contoso.Widget");
        var plain = Make("app.widget", new RemoveAppxPackage("Contoso.Widget"));
        _engine.Apply(plain, Guid.NewGuid());

        _apps.Provisioned.Add("Contoso.Widget");

        Assert.False(_scanner.Scan([plain], Pro).HasDrift);
        Assert.False(DeprovisionVariant.WasRecorded(_journal.OutstandingFor("app.widget")));
    }

    [Fact]
    public void An_app_installed_again_is_described_in_plain_words()
    {
        _apps.Add("Contoso.Widget");
        var plain = Make("app.widget", new RemoveAppxPackage("Contoso.Widget"));
        _engine.Apply(plain, Guid.NewGuid());
        _apps.Current.Add("Contoso.Widget");

        var item = Assert.Single(_scanner.Scan([plain], Pro).Items);

        Assert.Equal("Contoso.Widget is installed again", Assert.Single(item.Changed));
    }

    [Fact]
    public void A_setting_the_user_reverted_is_not_checked()
    {
        var tweak = Make("a", Dword("A", "0"));
        _engine.Apply(tweak, Guid.NewGuid());
        _engine.Revert(tweak, Guid.NewGuid());

        var report = _scanner.Scan([tweak], Pro);

        Assert.False(report.HasDrift);
        Assert.Equal(0, report.Checked);
    }

    [Fact]
    public void A_setting_that_was_never_applied_is_not_checked()
    {
        var report = _scanner.Scan([Make("a", Dword("A", "0"))], Pro);

        Assert.Equal(0, report.Checked);
        Assert.False(report.HasDrift);
    }

    [Fact]
    public void Settings_this_edition_does_not_support_are_skipped()
    {
        var tweak = Make("a", Dword("A", "0")) with { Requires = new Requirements { Editions = ["Enterprise"] } };
        _engine.Apply(tweak, Guid.NewGuid());
        WindowsSets("A", "1");

        Assert.Equal(0, _scanner.Scan([tweak], Pro).Checked);
    }

    [Fact]
    public void Stopping_tracking_hides_the_change_and_leaves_windows_alone()
    {
        var tweak = Make("a", Dword("A", "0"));
        _engine.Apply(tweak, Guid.NewGuid());
        WindowsSets("A", "1");
        Assert.True(_scanner.Scan([tweak], Pro).HasDrift);

        _engine.Release(tweak, Guid.NewGuid());

        Assert.False(_scanner.Scan([tweak], Pro).HasDrift);
        Assert.Equal("1", _reg.Read(T("A")).Data!.Value);
        Assert.DoesNotContain("a", _journal.TweakIdsWithOutstanding());
    }

    [Fact]
    public void Applying_again_after_stopping_tracking_starts_tracking_again()
    {
        var tweak = Make("a", Dword("A", "0"));
        _engine.Apply(tweak, Guid.NewGuid());
        WindowsSets("A", "1");
        _engine.Release(tweak, Guid.NewGuid());

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        WindowsSets("A", "1");

        Assert.True(_scanner.Scan([tweak], Pro).HasDrift);
    }

    [Fact]
    public void A_choice_replaced_by_another_in_its_exclusive_group_is_not_drift()
    {
        var first = Make("plan.one", Dword("Plan", "1")) with { ExclusiveGroup = "plan" };
        var second = Make("plan.two", Dword("Plan", "2")) with { ExclusiveGroup = "plan" };
        _engine.Apply(first, Guid.NewGuid());
        _engine.Apply(second, Guid.NewGuid());

        var report = _scanner.Scan([first, second], Pro);

        Assert.False(report.HasDrift);
    }

    [Fact]
    public void A_choice_in_an_exclusive_group_is_drift_when_nothing_replaced_it()
    {
        var first = Make("plan.one", Dword("Plan", "1")) with { ExclusiveGroup = "plan" };
        var second = Make("plan.two", Dword("Plan", "2")) with { ExclusiveGroup = "plan" };
        _engine.Apply(first, Guid.NewGuid());
        WindowsSets("Plan", "3");

        Assert.Equal("plan.one", Assert.Single(_scanner.Scan([first, second], Pro).Items).Tweak.Id);
    }

    [Fact]
    public void The_first_scan_has_no_earlier_build_and_a_later_one_sees_a_feature_update()
    {
        var tweak = Make("a", Dword("A", "0"));
        _engine.Apply(tweak, Guid.NewGuid());

        var first = _scanner.Scan([tweak], new MachineProfile(26100, "Professional"));
        Assert.Null(first.PreviousBuild);
        Assert.False(first.FeatureUpdateSinceLastScan);

        var same = _scanner.Scan([tweak], new MachineProfile(26100, "Professional"));
        Assert.False(same.FeatureUpdateSinceLastScan);

        var after = _scanner.Scan([tweak], new MachineProfile(26200, "Professional"));
        Assert.Equal(26100, after.PreviousBuild);
        Assert.True(after.FeatureUpdateSinceLastScan);
    }

    [Fact]
    public void A_damaged_state_file_is_treated_as_no_earlier_scan()
    {
        Directory.CreateDirectory(_dir);
        var file = Path.Combine(_dir, "drift.json");
        File.WriteAllText(file, "{ not json");
        var tweak = Make("a", Dword("A", "0"));
        _engine.Apply(tweak, Guid.NewGuid());

        var report = _scanner.Scan([tweak], Pro);

        Assert.Null(report.PreviousBuild);
        Assert.Equal(26100, new DriftState(file).Load().LastBuild);
    }

    [Fact]
    public void A_setting_that_cannot_be_read_is_listed_and_does_not_stop_the_scan()
    {
        var bad = Make("bad", new RemoveAppxPackage("Contoso.Broken"));
        var good = Make("good", Dword("A", "0"));
        var engine = new TweakEngine([new RegistryHandler(_reg), new ThrowingAppxHandler()], _journal);
        engine.Apply(good, Guid.NewGuid());
        _journal.Append(new JournalEntry(Guid.NewGuid(), "bad", JournalAction.Apply, DateTimeOffset.UtcNow,
            new RemoveAppxPackage("Contoso.Broken"), new RestoreAppxPackage("Contoso.Broken", null)));
        WindowsSets("A", "1");

        var report = new DriftScanner(engine, _journal).Scan([bad, good], Pro);

        Assert.Equal(["bad"], report.Unreadable);
        Assert.Equal("good", Assert.Single(report.Items).Tweak.Id);
    }

    [Fact]
    public void The_outcome_lines_list_what_holds_what_changed_back_and_what_could_not_be_read()
    {
        _apps.Add("Contoso.Widget");
        var holds = Make("a.holds", Dword("A", "0"));
        var back = Make("b.back", Dword("B", "0"));
        var app = Make("c.app", new RemoveAppxPackage("Contoso.Widget"));
        _engine.Apply(holds, Guid.NewGuid());
        _engine.Apply(back, Guid.NewGuid());
        _engine.Apply(app, Guid.NewGuid());
        WindowsSets("B", "1");
        _apps.Current.Add("Contoso.Widget");

        var lines = OutcomeReport.Lines(_scanner.Scan([holds, back, app], Pro));

        Assert.Equal(["Windows build 26100", "a.holds: holds", "b.back: changed back", "c.app: app came back"], lines);
    }

    [Fact]
    public void Outcome_lines_carry_only_ids_and_a_build_number()
    {
        var tweak = Make("a", Dword("A", "0"));
        _engine.Apply(tweak, Guid.NewGuid());

        var text = string.Join("\n", OutcomeReport.Lines(_scanner.Scan([tweak], Pro)));

        Assert.DoesNotContain(Environment.UserName, text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(Environment.MachineName, text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\\", text);
    }

    private sealed class ThrowingAppxHandler : IOperationHandler
    {
        public bool Handles(Operation op) => op is RemoveAppxPackage or RestoreAppxPackage;
        public bool IsApplicable(Operation op) => true;
        public bool IsSatisfied(Operation op) => throw new InvalidOperationException("PowerShell did not answer");
        public Operation CaptureUndo(Operation op) => throw new NotSupportedException();
        public void Execute(Operation op) => throw new NotSupportedException();
    }
}
