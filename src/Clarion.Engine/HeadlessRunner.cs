using Clarion.Core.Model;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Clarion.Core.Apps;
using Clarion.Core.Catalog;
using Clarion.Core.Cleanup;
using Clarion.Core.Engine;
using Clarion.Core.Profiles;

namespace Clarion.Engine;

/// <summary>Runs Clarion without a window for scripts and scheduled tasks. Returns the exit code.</summary>
[SupportedOSPlatform("windows")]
public static class HeadlessRunner
{
    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(int processId);

    /// <summary>Shows output in the terminal that started Clarion, when there is one.</summary>
    public static TextWriter OpenConsole()
    {
        try
        {
            AttachConsole(-1); // fails harmlessly when output is redirected to a file or pipe
            // The writer owns the stream and is handed to the caller, who disposes it.
#pragma warning disable CA2000
            var stream = Console.OpenStandardOutput();
            if (stream != Stream.Null) return new StreamWriter(stream) { AutoFlush = true };
#pragma warning restore CA2000
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException) { }
        return TextWriter.Null;
    }

    public static string Version =>
        (Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0").Split('+')[0];

    public static int Run(CliOptions o, TextWriter output, Action<string>? log = null)
    {
        void Say(string text) { output.WriteLine(text); log?.Invoke(text); }

        switch (o.Mode)
        {
            case CliMode.Help:
                if (o.Error is not null) Say(o.Error);
                Say(CliOptions.HelpText);
                return o.Error is null ? 0 : 2;
            case CliMode.Version:
                Say($".Clarion {Version}");
                return 0;
            case CliMode.ListPresets:
                foreach (var (name, ids) in CatalogLoader.LoadPresets()) Say($"{name} ({ids.Count} settings)");
                return 0;
            case CliMode.ListSettings:
                Say(Core.Reports.CliReports.Settings(CatalogLoader.LoadEmbedded()));
                return 0;
            case CliMode.ListStartup:
                Say(Core.Reports.CliReports.StartupEntries(Core.Startup.StartupList.Build(new WindowsStartupSource())));
                return 0;
            case CliMode.ListCleanups:
                Say(Core.Reports.CliReports.Cleanups(CatalogLoader.LoadCleanup()));
                return 0;
            case CliMode.CatalogDoc:
                output.Write(Core.Reports.CatalogDocument.Markdown(CatalogLoader.LoadEmbedded()));
                return 0;
            case CliMode.ListSymptoms:
                Say(Core.Reports.CliReports.Symptoms(CatalogLoader.LoadSymptoms()));
                return 0;
            case CliMode.Verify or CliMode.WhatBroke or CliMode.History:
                return RunReadOnly(o, Say);
            // Listing what is out of date only reads, so it needs no administrator rights. Updating does.
            case CliMode.UpdateApps when o.Preview:
                return RunUpdateApps(o, Say);
        }

        if (!WindowsMachine.IsElevated())
        {
            Say(".Clarion needs administrator rights. Start it from an elevated terminal.");
            return 3;
        }

        return o.Mode switch
        {
            CliMode.Apply => RunApply(o, Say),
            CliMode.UpdateApps => RunUpdateApps(o, Say),
            _ => RunClean(o, Say),
        };
    }

    /// <summary>
    /// Updates the installed apps of the Apps page that have a newer version in winget, or with --preview only lists them. With --all-apps it
    /// takes every app winget lists as out of date, each one pinned by the exact ID winget printed. It never acts on a name from the command
    /// line, and leaves apps alone when the command runs as another account than the one
    /// signed in, since winget would then work on that account's apps.
    /// </summary>
    private static int RunUpdateApps(CliOptions o, Action<string> say)
    {
        var apps = CatalogLoader.LoadEmbedded().Where(t => t.Apply.OfType<InstallWingetPackage>().Any()).ToList();
        var chosen = apps;
        if (o.Only is { Count: > 0 })
        {
            var unknown = o.Only.Where(n => apps.All(a => !a.Id.Equals(n, StringComparison.OrdinalIgnoreCase))).ToList();
            foreach (var n in unknown) say($"unknown app: {n}. Use --list-settings for the names.");
            if (unknown.Count > 0) return 2;
            chosen = apps.Where(a => o.Only.Contains(a.Id, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        var runtime = EngineFactory.Create();
        if (!runtime.Winget.IsAvailable)
        {
            say("winget was not found for this account, so apps cannot be updated. Install or repair App Installer from Microsoft.");
            return 1;
        }

        var byPackage = chosen.ToDictionary(a => a.Apply.OfType<InstallWingetPackage>().First().Id, a => a, StringComparer.OrdinalIgnoreCase);
        IReadOnlyList<AppUpdate> updates;
        try
        {
            updates = o.AllApps
                ? runtime.Winget.AllAvailableUpdates()
                : runtime.Winget.AvailableUpdates(byPackage.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase));
        }
        catch (InvalidOperationException ex) { say($"winget could not say what is out of date. {ex.Message}"); return 1; }

        if (updates.Count == 0)
        {
            say("Every app from the list that winget can read the version of is up to date. winget does not list an app whose installed version it cannot read, or one that is pinned.");
            return 0;
        }
        say($"{updates.Count} {(updates.Count == 1 ? "app has" : "apps have")} a newer version:");
        foreach (var u in updates) say($"  {(byPackage.TryGetValue(u.Id, out var known) ? known.Name : u.Name.Length > 0 ? u.Name : u.Id)}: {u.Current} -> {u.Available}");
        if (o.Preview) return 0;

        if (AccountCheck.Detect() is { Differs: true } account)
        {
            say($"This command runs as {account.ElevatedAs}, but {account.SignedInAs} is signed in, so winget would work on the wrong account's apps. Start the command again from your own account.");
            return 1;
        }

        var results = AppUpdater.Run(runtime.Winget, updates, o.AcceptPackageAgreements, say, CancellationToken.None);
        say(AppUpdater.Summary(results) + ".");
        if (results.Any(r => !r.Succeeded && !r.Skipped && r.Message.Contains("licence terms", StringComparison.Ordinal)))
            say("Add --accept-package-agreements to accept the publisher's licence terms for the apps that ask for them again.");
        if (runtime.Winget.TakeRestartNeeded()) say("Restart Windows to finish some of these.");
        return results.All(r => r.Succeeded) ? 0 : 1;
    }

    /// <summary>The commands that only look. They change nothing in Windows, so they do not ask for administrator rights.</summary>
    private static int RunReadOnly(CliOptions o, Action<string> say)
    {
        var runtime = EngineFactory.Create();
        var catalog = CatalogLoader.LoadEmbedded();

        switch (o.Mode)
        {
            case CliMode.Verify:
                var report = runtime.Drift.Scan(catalog.Concat(Core.Startup.StartupTweaks.ForIds(runtime.Journal.OutstandingByTweak().Keys)).ToList(), runtime.Profile);
                if (o.SaveReport)
                    new Core.Scheduling.ScheduledVerifyStore(Path.Combine(EngineFactory.DefaultDataDirectory, "scheduled-verify.json"))
                        .Save(Core.Scheduling.ScheduledVerifyResult.From(report, DateTimeOffset.Now));
                say(Core.Reports.CliReports.Verify(report, Core.SystemInfo.PendingRestart.Check(new WindowsPendingRestartProbe())));
                return Core.Reports.CliReports.VerifyExitCode(report, o.SaveReport);

            case CliMode.WhatBroke:
                var symptom = CatalogLoader.LoadSymptoms().FirstOrDefault(s => s.Id.Equals(o.Symptom, StringComparison.OrdinalIgnoreCase));
                if (symptom is null) { say($"There is no symptom called {o.Symptom}. Use --list-symptoms."); return 2; }
                var suspects = new Core.Troubleshoot.Troubleshooter(runtime.Journal).Suspects(symptom, catalog.Concat(Core.Startup.StartupTweaks.ForIds(runtime.Journal.OutstandingByTweak().Keys)).ToList(), o.Since);
                say(Core.Reports.CliReports.WhatBroke(symptom, suspects, o.Since));
                return 0;

            default:
                say(Core.Reports.CliReports.History(runtime.Journal.OutstandingByTweak(), catalog));
                return 0;
        }
    }

    private static int RunApply(CliOptions o, Action<string> say)
    {
        IEnumerable<string> ids;
        if (o.File is not null)
        {
            var load = ProfileFile.Load(o.File);
            if (load.Profile is null) { say(load.Error ?? "Could not read the setup file."); return 2; }
            ids = load.Profile.Tweaks;
        }
        else
        {
            var presets = CatalogLoader.LoadPresets();
            if (!presets.TryGetValue(o.Preset ?? "", out var list)) { say($"There is no preset called {o.Preset}. Use --list-presets."); return 2; }
            ids = list;
        }

        var runtime = EngineFactory.Create();
        var catalog = CatalogLoader.LoadEmbedded();
        // Only what was asked for is read up front. Warming the whole catalog would also run winget and the feature and app queries
        // for settings that are not part of this run.
        var wanted = ids.ToHashSet(StringComparer.OrdinalIgnoreCase);
        runtime.Engine.WarmUp(catalog.Where(t => wanted.Contains(t.Id)));
        var plan = CliPlanner.Plan(ids, catalog, runtime.Profile, runtime.Engine.Detect);

        // Apps are installed only when the person has said, on the command line, that they accept the publishers' licence terms.
        var skippedApps = new List<Tweak>();
        if (!o.AcceptPackageAgreements)
        {
            skippedApps = plan.ToApply.Where(t => t.Apply.OfType<InstallWingetPackage>().Any()).ToList();
            plan = plan with { ToApply = plan.ToApply.Except(skippedApps).ToList() };
        }
        runtime.Winget.AcceptPackageAgreements = o.AcceptPackageAgreements;

        // winget runs as the account that runs this command. When that is an administrator account other than the one that is signed in,
        // an app would be installed into that other account's profile, so apps are left out, as the Apps page leaves them out.
        var blockedApps = new List<Tweak>();
        var wingetSettings = plan.ToApply.Where(t => t.Apply.Any(op => op is InstallWingetPackage or UninstallWingetPackage)).ToList();
        if (wingetSettings.Count > 0 && AccountCheck.Detect() is { Differs: true } account)
        {
            blockedApps = wingetSettings;
            plan = plan with { ToApply = plan.ToApply.Except(blockedApps).ToList() };
            say($"This command runs as {account.ElevatedAs}, but {account.SignedInAs} is signed in, so apps would be installed for the wrong account. Start the command again from your own account to include them.");
        }

        // Apps that go in one account only: each one that supports it. The others are installed for everyone, and the list says which.
        var forEveryone = new List<Tweak>();
        if (o.UserScope)
        {
            var scopes = AppScopes.Default;
            var apps = plan.ToApply.Where(t => t.Apply.OfType<InstallWingetPackage>().Any()).ToList();
            forEveryone = apps.Where(t => !UserScopeVariant.CanApply(t, scopes)).ToList();
            plan = plan with { ToApply = plan.ToApply.Select(t => UserScopeVariant.CanApply(t, scopes) ? UserScopeVariant.Of(t) : t).ToList() };
        }

        say($"{plan.ToApply.Count} to turn on, {plan.AlreadyOn.Count} already on, {plan.NotSupported.Count} not available here, {plan.Unknown.Count} unknown.");
        foreach (var id in plan.Unknown) say($"  unknown setting: {id}");
        foreach (var id in plan.NotSupported) say($"  not available here: {id}{(plan.Reasons.TryGetValue(id, out var why) ? $" ({why})" : "")}");
        foreach (var t in plan.ToApply)
            say($"  {(o.Preview ? "would turn on" : "turning on")}: {t.Name}{(t.Apply.OfType<InstallWingetPackage>().Any(i => i.Scope == "user") ? " (for this account only)" : "")}");
        foreach (var t in forEveryone) say($"  {t.Name} has no option for one account, so it is installed for everyone.");
        foreach (var t in skippedApps) say($"  skipped app: {t.Name}. Add --accept-package-agreements to install it, which accepts the publisher's licence terms for you.");
        foreach (var t in blockedApps) say($"  skipped app: {t.Name}. It would go to a different account.");
        if (o.Preview || plan.ToApply.Count == 0) return skippedApps.Count + blockedApps.Count > 0 ? 1 : 0;

        var result = runtime.Runner.Apply(plan.ToApply, runtime.Profile,
            new BatchOptions { CreateRestorePoint = !o.NoRestorePoint, ContinueWithoutRestorePoint = o.NoRestorePoint, TurnOnSystemProtection = o.EnableProtection }, say);
        if (result.Blocked is not null)
        {
            say(result.Blocked);
            if (!o.EnableProtection) say("If System Protection is off, add --enable-protection to turn it on. Or add --no-restore-point to continue without one.");
            return 1;
        }
        foreach (var item in result.Items.Where(i => !i.Result.Success)) say($"  failed: {item.TweakId}: {item.Result.Error}");
        // An installer can ask for a restart even though the setting itself does not need one.
        if (runtime.Winget.TakeRestartNeeded()) say("Restart Windows to finish some of these.");
        var complete = result.AllSucceeded && skippedApps.Count == 0 && blockedApps.Count == 0;
        say(complete ? "Done." : "Some settings did not finish.");
        return complete ? 0 : 1;
    }

    private static int RunClean(CliOptions o, Action<string> say)
    {
        var platform = new WindowsCleanupPlatform();
        var engine = new CleanupEngine(platform);
        var targets = CatalogLoader.LoadCleanup();

        var wanted = o.Only is { Count: > 0 }
            ? targets.Where(t => o.Only.Contains(t.Id, StringComparer.OrdinalIgnoreCase)).ToList()
            : targets.Where(t => t.DefaultOn || (o.Include?.Contains(t.Id, StringComparer.OrdinalIgnoreCase) ?? false)).ToList();
        var unknown = (o.Only ?? []).Concat(o.Include ?? []).Where(n => targets.All(t => !t.Id.Equals(n, StringComparison.OrdinalIgnoreCase))).ToList();
        foreach (var n in unknown) say($"unknown cleanup row: {n}");
        if (unknown.Count > 0) return 2;

        long freed = 0;
        var locked = 0;
        foreach (var t in wanted)
        {
            var r = engine.Clean(t, queueLocked: true, preview: o.Preview, null, CancellationToken.None);
            freed += r.BytesFreed;
            locked += r.FilesLocked;
            say($"{t.Id,-24} {ByteSize.Format(r.BytesFreed),10}{(r.Skipped ? "  skipped: " + string.Join(" ", r.Notes) : "")}");
        }
        say($"{(o.Preview ? "Would free" : "Freed")} {ByteSize.Format(freed)}.");
        return locked > 0 ? 1 : 0;
    }
}
