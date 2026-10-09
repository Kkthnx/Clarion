using Clarion.Core.Model;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
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
            var stream = Console.OpenStandardOutput();
            if (stream != Stream.Null) return new StreamWriter(stream) { AutoFlush = true };
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
                Say($"Clarion {Version}");
                return 0;
            case CliMode.ListPresets:
                foreach (var (name, ids) in CatalogLoader.LoadPresets()) Say($"{name} ({ids.Count} settings)");
                return 0;
        }

        if (!WindowsMachine.IsElevated())
        {
            Say("Clarion needs administrator rights. Start it from an elevated terminal.");
            return 3;
        }

        return o.Mode == CliMode.Apply ? RunApply(o, Say) : RunClean(o, Say);
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
        runtime.Engine.WarmUp(catalog);
        var plan = CliPlanner.Plan(ids, catalog, runtime.Profile, runtime.Engine.Detect);

        say($"{plan.ToApply.Count} to turn on, {plan.AlreadyOn.Count} already on, {plan.NotSupported.Count} not available here, {plan.Unknown.Count} unknown.");
        foreach (var id in plan.Unknown) say($"  unknown setting: {id}");
        foreach (var id in plan.NotSupported) say($"  not available here: {id}");
        foreach (var t in plan.ToApply) say($"  {(o.Preview ? "would turn on" : "turning on")}: {t.Name}");
        if (o.Preview || plan.ToApply.Count == 0) return 0;

        var result = runtime.Runner.Apply(plan.ToApply, runtime.Profile,
            new BatchOptions { CreateRestorePoint = !o.NoRestorePoint, ContinueWithoutRestorePoint = o.NoRestorePoint, TurnOnSystemProtection = o.EnableProtection }, say);
        if (result.Blocked is not null)
        {
            say(result.Blocked);
            if (!o.EnableProtection) say("If System Protection is off, add --enable-protection to turn it on. Or add --no-restore-point to continue without one.");
            return 1;
        }
        foreach (var item in result.Items.Where(i => !i.Result.Success)) say($"  failed: {item.TweakId}: {item.Result.Error}");
        say(result.AllSucceeded ? "Done." : "Some settings did not finish.");
        return result.AllSucceeded ? 0 : 1;
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
