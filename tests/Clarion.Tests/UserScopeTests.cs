using System.Text.Json;
using Clarion.Core.Apps;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Profiles;

namespace Clarion.Tests;

public sealed class UserScopeTests : IDisposable
{
    private const string Alice = "S-1-5-21-1-1-1-1001";
    private const string Bob = "S-1-5-21-1-1-1-1002";

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-scope-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak App(string id = "app.git", string package = "Git.Git") => new()
    {
        Id = id,
        Category = "Install",
        Topic = "Development",
        Name = "Git",
        Summary = "s",
        What = "w",
        Benefit = "b",
        Risk = "r",
        Evidence = Evidence.Proven,
        RiskLevel = RiskLevel.Low,
        Scope = TweakScope.Machine,
        Apply = [new InstallWingetPackage(package)],
        Undo = [new UninstallWingetPackage(package)],
    };

    [Fact]
    public void An_uninstaller_that_opens_its_own_wizard_is_reported_as_still_installed_with_a_plain_way_forward()
    {
        // winget uses the quiet uninstall command only when the app registered one, and otherwise runs the interactive uninstaller and says it worked.
        var store = new FakeWingetStore { UninstallLeavesItInstalled = true };
        store.Installed.Add("Xournal++.Xournal++");
        var waited = new List<TimeSpan>();

        var error = Assert.Throws<InvalidOperationException>(() => new WingetHandler(store, waited.Add).Execute(new UninstallWingetPackage("Xournal++.Xournal++", null, "user")));

        Assert.Contains("still installed", error.Message);
        Assert.Contains("own window", error.Message);
        Assert.Contains("Re-check", error.Message);
        Assert.Equal(2, waited.Count);
        Assert.Equal(["uninstall Xournal++.Xournal++ scope user"], store.Calls);
    }

    [Fact]
    public void An_uninstaller_that_finishes_a_moment_after_winget_returns_counts_as_done()
    {
        var store = new FakeWingetStore { GoneAfterReads = 2 };
        store.Installed.Add("Some.App");
        var waited = new List<TimeSpan>();

        new WingetHandler(store, waited.Add).Execute(new UninstallWingetPackage("Some.App"));

        Assert.Single(waited);
        Assert.DoesNotContain("Some.App", store.InstalledIds());
    }

    [Fact]
    public void A_normal_uninstall_does_not_wait()
    {
        var store = new FakeWingetStore();
        store.Installed.Add("Some.App");
        var waited = new List<TimeSpan>();
        new WingetHandler(store, waited.Add).Execute(new UninstallWingetPackage("Some.App"));
        Assert.Empty(waited);
    }

    [Fact]
    public void The_commands_add_the_scope_only_when_one_is_given_and_only_user_or_machine_can_get_through()
    {
        Assert.DoesNotContain("--scope", WingetCommands.Install("Git.Git"));
        Assert.EndsWith(" --scope user", WingetCommands.Install("Git.Git", null, "user"));
        Assert.EndsWith(" --scope machine", WingetCommands.Uninstall("Git.Git", "machine"));
        Assert.DoesNotContain("--scope", WingetCommands.Uninstall("Git.Git"));
        Assert.Throws<ArgumentException>(() => WingetCommands.Install("Git.Git", null, "user --force"));
        Assert.Throws<ArgumentException>(() => WingetCommands.Uninstall("Git.Git", "everyone"));
    }

    [Fact]
    public void The_scope_goes_to_winget_and_the_undo_that_is_recorded_is_for_the_same_account()
    {
        var store = new FakeWingetStore { AcceptPackageAgreements = true };
        var engine = new TweakEngine([new WingetHandler(store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl"), Alice));
        var forMe = UserScopeVariant.Of(App());

        Assert.True(engine.Apply(forMe, Guid.NewGuid()).Success);
        Assert.Equal(["install Git.Git scope user"], store.Calls);

        Assert.True(engine.Revert(App(), Guid.NewGuid()).Success);
        Assert.Equal(["install Git.Git scope user", "uninstall Git.Git scope user"], store.Calls);
    }

    [Fact]
    public void An_install_for_everyone_is_unchanged_and_its_undo_has_no_scope()
    {
        var store = new FakeWingetStore { AcceptPackageAgreements = true };
        var engine = new TweakEngine([new WingetHandler(store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));

        engine.Apply(App(), Guid.NewGuid());
        engine.Revert(App(), Guid.NewGuid());

        Assert.Equal(["install Git.Git", "uninstall Git.Git"], store.Calls);
    }

    [Fact]
    public void The_undo_of_each_winget_step_keeps_the_folder_and_the_scope()
    {
        var handler = new WingetHandler(new FakeWingetStore());
        var undo = Assert.IsType<UninstallWingetPackage>(handler.CaptureUndo(new InstallWingetPackage("A.B", "%ProgramFiles%\\A", "user")));
        Assert.Equal(("A.B", "%ProgramFiles%\\A", "user"), (undo.Id, undo.Location, undo.Scope));

        var back = Assert.IsType<InstallWingetPackage>(handler.CaptureUndo(new UninstallWingetPackage("A.B", null, "user")));
        Assert.Equal("user", back.Scope);
    }

    [Fact]
    public void An_app_installed_for_one_account_belongs_to_that_account_in_the_history()
    {
        Assert.True(ChangeJournal.IsPerUser(new InstallWingetPackage("Git.Git", null, "user")));
        Assert.True(ChangeJournal.IsPerUser(new UninstallWingetPackage("Git.Git", null, "user")));
        Assert.False(ChangeJournal.IsPerUser(new InstallWingetPackage("Git.Git")));
        Assert.False(ChangeJournal.IsPerUser(new UninstallWingetPackage("Git.Git")));

        var path = Path.Combine(_dir, "owner.jsonl");
        var op = new InstallWingetPackage("Git.Git", null, "user");
        new ChangeJournal(path, Alice).Append(new JournalEntry(Guid.NewGuid(), "app.git", JournalAction.Apply, DateTimeOffset.UtcNow, op, new UninstallWingetPackage("Git.Git", null, "user")));

        Assert.Equal(Alice, Assert.Single(new ChangeJournal(path, Alice).ReadAll()).UserSid);
        // Another administrator opening .Clarion does not see Alice's profile as theirs, so they cannot revert it.
        Assert.Empty(new ChangeJournal(path, Bob).OutstandingByTweak());
        Assert.Single(new ChangeJournal(path, Alice).OutstandingByTweak());
    }

    [Fact]
    public void The_variant_changes_only_the_scope_of_the_winget_steps()
    {
        var plain = new Tweak
        {
            Id = "app.x",
            Category = "Install",
            Topic = "t",
            Name = "n",
            Summary = "s",
            What = "w",
            Benefit = "b",
            Risk = "r",
            Evidence = Evidence.Proven,
            RiskLevel = RiskLevel.Low,
            Scope = TweakScope.Machine,
            Apply = [new InstallWingetPackage("A.B", "%ProgramFiles%\\A")],
            Undo = [new UninstallWingetPackage("A.B", "%ProgramFiles%\\A")],
        };
        var variant = UserScopeVariant.Of(plain);

        Assert.Equal(new InstallWingetPackage("A.B", "%ProgramFiles%\\A", "user"), Assert.Single(variant.Apply));
        Assert.Equal(new UninstallWingetPackage("A.B", "%ProgramFiles%\\A", "user"), Assert.Single(variant.Undo));
        Assert.Equal(plain.Id, variant.Id);
        Assert.Equal(TweakScope.Machine, variant.Scope);
        // The catalog entry itself is untouched.
        Assert.Null(((InstallWingetPackage)plain.Apply[0]).Scope);
    }

    [Fact]
    public void An_app_may_go_in_one_account_only_when_it_is_listed_and_installs_a_winget_package()
    {
        var listed = AppScopes.Default.All.Keys.FirstOrDefault();
        if (listed is null) return;
        var real = CatalogLoader.LoadEmbedded().Single(t => t.Id == listed);
        Assert.True(UserScopeVariant.CanApply(real, AppScopes.Default));

        var notListed = CatalogLoader.LoadEmbedded().First(t => t.Apply.OfType<InstallWingetPackage>().Any() && !AppScopes.Default.SupportsUser(t.Id));
        Assert.False(UserScopeVariant.CanApply(notListed, AppScopes.Default));
        var notAnApp = CatalogLoader.LoadEmbedded().First(t => !t.Apply.OfType<InstallWingetPackage>().Any());
        Assert.False(UserScopeVariant.CanApply(notAnApp, AppScopes.Default));
    }

    [Fact]
    public void Every_listed_app_exists_installs_a_package_and_says_why_it_is_listed()
    {
        var catalog = CatalogLoader.LoadEmbedded().ToDictionary(t => t.Id, StringComparer.Ordinal);
        foreach (var (id, scope) in AppScopes.Default.All)
        {
            Assert.True(catalog.TryGetValue(id, out var tweak), $"{id} is not in the catalog");
            Assert.Contains(tweak!.Apply, o => o is InstallWingetPackage);
            Assert.Contains(scope.Basis, new[] { "declared", "portable", "msix" });
            // A package winget does not carry the scope out for must have a manifest that declares it.
            if (scope.Basis is "portable" or "msix") Assert.Equal(scope.Basis, scope.InstallerType);
            Assert.False(string.IsNullOrWhiteSpace(scope.InstallerType), id);
        }
    }

    [Fact]
    public void A_journal_line_with_no_scope_still_reads_and_one_with_a_scope_round_trips()
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var old = JsonSerializer.Deserialize<Operation>("{\"type\":\"winget.install\",\"id\":\"Git.Git\"}", CatalogLoader.Options);
        Assert.Equal(new InstallWingetPackage("Git.Git"), old);

        var json = JsonSerializer.Serialize<Operation>(new InstallWingetPackage("Git.Git", null, "user"), CatalogLoader.Options);
        Assert.Contains("\"scope\":\"user\"", json);
        Assert.Equal(new InstallWingetPackage("Git.Git", null, "user"), JsonSerializer.Deserialize<Operation>(json, CatalogLoader.Options));
        Assert.NotNull(options);
    }

    [Fact]
    public void The_descriptions_say_when_an_install_or_removal_is_for_one_account()
    {
        Assert.Equal("Install Git.Git with winget for this account only", new InstallWingetPackage("Git.Git", null, "user").Describe());
        Assert.Equal("Install Git.Git with winget", new InstallWingetPackage("Git.Git").Describe());
        Assert.Equal("Uninstall Git.Git with winget, from this account", new UninstallWingetPackage("Git.Git", null, "user").Describe());
    }

    [Fact]
    public void The_command_line_switch_is_read_and_off_unless_given()
    {
        Assert.True(CliOptions.Parse(["--apply-preset", "standard", "--user-scope"]).UserScope);
        Assert.False(CliOptions.Parse(["--apply-preset", "standard"]).UserScope);
        Assert.Contains("--user-scope", CliOptions.HelpText);
    }
}
