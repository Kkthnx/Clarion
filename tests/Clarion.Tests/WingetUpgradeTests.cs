using Clarion.Core.Apps;

namespace Clarion.Tests;

public sealed class WingetUpgradeTests
{
    private static readonly IReadOnlySet<string> Known = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "CPUID.CPU-Z", "Microsoft.DotNet.SDK.9", "Microsoft.Edge", "Microsoft.DotNet.DesktopRuntime.10", "Git.Git", "Foo.Bar",
    };

    // What winget upgrade --source winget printed on a Windows 11 PC, with the lines it wrote and no spinner.
    private const string Real =
        "Name                                            Id                                 Version            Available\r\n" +
        "------------------------------------------------------------------------------------------------------------------------\r\n" +
        "App Installer                                   Microsoft.AppInstaller             1.29.379.0         1.29.380\r\n" +
        "CPUID CPU-Z 2.19                                CPUID.CPU-Z                        2.19               3.01\r\n" +
        "Hytale Launcher                                 HypixelStudios.Hytale              2026.09.30-326a0c2 2026.10.14-4344467\r\n" +
        "Microsoft .NET SDK 9.0.318 (x64)                Microsoft.DotNet.SDK.9             9.0.318            9.0.319\r\n" +
        "Microsoft Edge                                  Microsoft.Edge                     155.0.4283.45      155.0.4283.60\r\n" +
        "Microsoft GameInput                             Microsoft.GameInput                3.5.270.0          3.5.283\r\n" +
        "Microsoft Windows Desktop Runtime 10.0.12 (x64) Microsoft.DotNet.DesktopRuntime.10 10.0.12            10.0.13\r\n" +
        "7 upgrades available.\r\n" +
        "2 package(s) have version numbers that cannot be determined. Use --include-unknown to see all results.\r\n";

    [Fact]
    public void The_table_winget_really_printed_gives_the_catalog_apps_that_are_out_of_date_and_no_others()
    {
        var updates = WingetUpgrades.Parse(Real, Known);

        Assert.Equal(
            ["CPUID.CPU-Z 2.19 3.01", "Microsoft.DotNet.SDK.9 9.0.318 9.0.319", "Microsoft.Edge 155.0.4283.45 155.0.4283.60", "Microsoft.DotNet.DesktopRuntime.10 10.0.12 10.0.13"],
            updates.Select(u => $"{u.Id} {u.Current} {u.Available}").ToArray());
    }

    [Fact]
    public void A_name_that_runs_into_the_id_column_does_not_hide_the_row()
    {
        // "(x64) Microsoft.DotNet.DesktopRuntime.10" has one space where the name ends and the id begins.
        var update = Assert.Single(WingetUpgrades.Parse(Real, new HashSet<string> { "Microsoft.DotNet.DesktopRuntime.10" }));
        Assert.Equal("10.0.13", update.Available);
    }

    [Fact]
    public void The_column_titles_and_closing_sentences_may_be_in_any_language()
    {
        var spanish = Real
            .Replace("Name   ", "Nombre ").Replace("Version            Available", "Versión            Disponible")
            .Replace("7 upgrades available.", "7 actualizaciones disponibles.")
            .Replace("2 package(s) have version numbers that cannot be determined. Use --include-unknown to see all results.", "No se pudo determinar la versión de 2 paquetes.");
        Assert.Equal(4, WingetUpgrades.Parse(spanish, Known).Count);
    }

    [Fact]
    public void A_source_column_after_the_versions_is_ignored()
    {
        var withSource = Real.Replace("3.01\r\n", "3.01  winget\r\n");
        var cpuz = WingetUpgrades.Parse(withSource, Known).Single(u => u.Id == "CPUID.CPU-Z");
        Assert.Equal(("2.19", "3.01"), (cpuz.Current, cpuz.Available));
    }

    [Theory]
    [InlineData("Foo Foo.Bar < 1.2.3 1.3.0 winget")]
    [InlineData("Foo Foo.Bar <1.2.3 1.3.0 winget")]
    [InlineData("Foo Foo.Bar > 1.2.3 1.3.0")]
    public void A_version_printed_with_a_leading_sign_is_read_without_it(string row)
    {
        var text = "Name Id Version Available Source\r\n" + new string('-', 60) + "\r\n" + row + "\r\n";
        var update = Assert.Single(WingetUpgrades.Parse(text, Known));
        Assert.Equal(("Foo.Bar", "1.2.3", "1.3.0"), (update.Id, update.Current, update.Available));
    }

    [Fact]
    public void A_spinner_before_the_table_and_a_row_whose_versions_match_change_nothing()
    {
        var text = "   -\r\n   \\\r\n   |\r\n" + Real.Replace("2.19               3.01", "3.01               3.01");
        var ids = WingetUpgrades.Parse(text, Known).Select(u => u.Id).ToArray();
        Assert.DoesNotContain("CPUID.CPU-Z", ids);
        Assert.Equal(3, ids.Length);
    }

    [Theory]
    [InlineData("No installed package found matching input criteria.\r\n")]
    [InlineData("")]
    [InlineData("Name Id Version Available\r\n")]
    public void Output_with_no_table_means_nothing_to_update(string text) =>
        Assert.Empty(WingetUpgrades.Parse(text, Known));

    [Fact]
    public void An_id_that_winget_cut_short_is_matched_by_its_start_only_when_that_is_unambiguous()
    {
        var header = "Name Id Version Available\r\n" + new string('-', 60) + "\r\n";
        var cut = WingetUpgrades.Parse(header + "Some long name Microsoft.DotNet.DesktopRu… 10.0.12 10.0.13\r\n", Known);
        Assert.Equal("Microsoft.DotNet.DesktopRuntime.10", Assert.Single(cut).Id);

        // Two known ids start the same way, so nothing is guessed.
        var two = new HashSet<string> { "Microsoft.DotNet.SDK.8", "Microsoft.DotNet.SDK.9" };
        Assert.Empty(WingetUpgrades.Parse(header + "Some name Microsoft.DotNet.SD… 9.0.1 9.0.2\r\n", two));

        // Too short to mean anything.
        Assert.Empty(WingetUpgrades.Parse(header + "Some name Mi… 1 2\r\n", Known));
    }

    [Fact]
    public void The_id_is_returned_in_the_spelling_the_catalog_uses()
    {
        var text = "Name Id Version Available\r\n" + new string('-', 40) + "\r\nGit git.git 2.1 2.2\r\n";
        Assert.Equal("Git.Git", Assert.Single(WingetUpgrades.Parse(text, Known)).Id);
    }

    [Fact]
    public void The_commands_pin_the_package_and_accept_licence_terms_only_when_told_to()
    {
        Assert.Equal("upgrade --source winget --accept-source-agreements --disable-interactivity", WingetCommands.ListUpgrades());
        Assert.Equal("upgrade --id Git.Git --exact --source winget --accept-source-agreements --disable-interactivity --silent", WingetCommands.Upgrade("Git.Git", false));
        Assert.EndsWith("--silent --accept-package-agreements", WingetCommands.Upgrade("Git.Git", true));
        Assert.Throws<ArgumentException>(() => WingetCommands.Upgrade("Git.Git --all", false));
        Assert.DoesNotContain("--include-unknown", WingetCommands.ListUpgrades());
        Assert.DoesNotContain("--include-pinned", WingetCommands.ListUpgrades());
    }

    [Fact]
    public void An_app_that_was_already_the_newest_counts_as_up_to_date_and_the_messages_say_updated_not_installed()
    {
        Assert.True(WingetCodes.IsUpgradeSuccess(0));
        Assert.True(WingetCodes.IsUpgradeSuccess(WingetCodes.UpdateNotApplicable));
        Assert.False(WingetCodes.IsUpgradeSuccess(unchecked((int)0x8A150011)));
        Assert.Equal("It is already the newest version.", WingetCodes.ExplainUpgrade(WingetCodes.UpdateNotApplicable));
        Assert.Contains("nothing was updated", WingetCodes.ExplainUpgrade(unchecked((int)0x8A150041)));
        Assert.Contains("pinned", WingetCodes.ExplainUpgrade(unchecked((int)0x8A150068)));
    }

    [Fact]
    public void Updates_run_in_order_a_failure_does_not_stop_the_next_and_the_summary_counts_each_kind()
    {
        var store = new FakeWingetStore();
        var chosen = new[] { new AppUpdate("A.A", "1", "2"), new AppUpdate("B.B", "1", "2"), new AppUpdate("C.C", "1", "2") };
        store.Updates.AddRange(chosen);
        var codes = new Queue<int>([0, unchecked((int)0x8A150101), 0]);
        var said = new List<string>();

        var results = AppUpdater.Run(new CodeSequenceStore(store, codes), chosen, false, said.Add, CancellationToken.None);

        Assert.Equal(["upgrade A.A", "upgrade B.B", "upgrade C.C"], store.Calls);
        Assert.Equal([true, false, true], results.Select(r => r.Succeeded).ToArray());
        Assert.Equal("The app is running. Close it and try again.", results[1].Message);
        Assert.Equal("2 updated, 1 failed", AppUpdater.Summary(results));
        Assert.Contains(said, s => s.StartsWith("Updating A.A from 1 to 2"));
    }

    [Fact]
    public void Stopping_ends_the_run_and_the_apps_after_it_are_reported_as_not_started()
    {
        var store = new FakeWingetStore();
        var chosen = new[] { new AppUpdate("A.A", "1", "2"), new AppUpdate("B.B", "1", "2") };
        using var stop = new CancellationTokenSource();
        var stopAfterFirst = new StopAfterFirstStore(store, stop);

        var results = AppUpdater.Run(stopAfterFirst, chosen, false, _ => { }, stop.Token);

        Assert.Equal(["upgrade A.A"], store.Calls);
        Assert.True(results[1].Skipped);
        Assert.Equal("1 updated, 1 not started", AppUpdater.Summary(results));
        Assert.True(store.Cancels >= 1);
    }

    [Fact]
    public void The_licence_choice_reaches_winget_and_a_store_that_throws_ends_only_that_app()
    {
        var store = new FakeWingetStore();
        AppUpdater.Run(store, [new AppUpdate("A.A", "1", "2")], true, _ => { }, CancellationToken.None);
        Assert.True(store.LastUpgradeAcceptedAgreements);

        var results = AppUpdater.Run(new ThrowingStore(store), [new AppUpdate("A.A", "1", "2"), new AppUpdate("B.B", "1", "2")], false, _ => { }, CancellationToken.None);
        Assert.All(results, r => Assert.False(r.Succeeded));
        Assert.Equal(2, results.Count);
    }

    // Wrappers that change what Upgrade does and pass everything else to the fake.
    private class Wrap : IWingetStore
    {
        protected FakeWingetStore Inner { get; }

        protected Wrap(FakeWingetStore fake) => Inner = fake;

        public bool IsAvailable => Inner.IsAvailable;
        public bool AcceptPackageAgreements { get => Inner.AcceptPackageAgreements; set => Inner.AcceptPackageAgreements = value; }
        public IReadOnlySet<string> InstalledIds() => Inner.InstalledIds();
        public void Invalidate() => Inner.Invalidate();
        public WingetOutcome Install(string id, string? location = null, string? scope = null) => Inner.Install(id, location, scope);
        public WingetOutcome Uninstall(string id, string? scope = null) => Inner.Uninstall(id, scope);
        public void CancelRunning() => Inner.CancelRunning();
        public bool TakeRestartNeeded() => Inner.TakeRestartNeeded();
        public WingetPackageInfo? Show(string id) => Inner.Show(id);
        public IReadOnlyList<AppUpdate> AvailableUpdates(IReadOnlySet<string> ids) => Inner.AvailableUpdates(ids);
        public virtual WingetOutcome Upgrade(string id, bool accept) => Inner.Upgrade(id, accept);
    }

    private sealed class CodeSequenceStore(FakeWingetStore fake, Queue<int> codes) : Wrap(fake)
    {
        public override WingetOutcome Upgrade(string id, bool accept) { Inner.UpgradeCode = codes.Dequeue(); return base.Upgrade(id, accept); }
    }

    private sealed class StopAfterFirstStore(FakeWingetStore fake, CancellationTokenSource stop) : Wrap(fake)
    {
        public override WingetOutcome Upgrade(string id, bool accept) { var r = base.Upgrade(id, accept); stop.Cancel(); return r; }
    }

    private sealed class ThrowingStore(FakeWingetStore fake) : Wrap(fake)
    {
        public override WingetOutcome Upgrade(string id, bool accept) => throw new InvalidOperationException("winget went away");
    }
}
