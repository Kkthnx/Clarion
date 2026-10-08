using System.Runtime.Versioning;
using Clarion.Core.Actions;
using Clarion.Core.Catalog;
using Clarion.Core.Model;
using Clarion.Engine;

namespace Clarion.Tests;

public sealed class FakeStreamingRunner : IStreamingRunner
{
    public List<string> Calls { get; } = [];
    public Dictionary<string, int> ExitCodes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Func<string, bool>? CancelOn { get; set; }
    public string[] Lines { get; set; } = [];

    public Task<int> RunAsync(string systemTool, string args, bool utf16, Action<string> onLine, CancellationToken cancel)
    {
        var key = $"{systemTool} {args}".Trim();
        Calls.Add(key);
        foreach (var l in Lines) onLine(l);
        if (CancelOn?.Invoke(key) == true) throw new OperationCanceledException();
        return Task.FromResult(ExitCodes.TryGetValue(key, out var code) ? code : 0);
    }
}

public sealed class ActionTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "clarion-act-" + Guid.NewGuid().ToString("N"));
    private readonly FakeStreamingRunner _fake = new();
    private readonly ActionRunner _runner;
    private readonly List<string> _log = [];

    public ActionTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "Windows"));
        _runner = new ActionRunner(_fake, s => s
            .Replace("%SystemRoot%", Path.Combine(_root, "Windows"), StringComparison.OrdinalIgnoreCase)
            .Replace("%TEMP%", Path.Combine(_root, "Temp"), StringComparison.OrdinalIgnoreCase)
            .Replace("%SystemDrive%", "C:", StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }

    private static ActionDef Make(IReadOnlyList<ActionStep> steps, IReadOnlyList<ActionStep>? always = null) => new()
    {
        Id = "a.one", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r", RiskLevel = RiskLevel.Safe,
        Advice = "a", Facts = ["one", "two"], Steps = steps, Always = always ?? [],
    };

    [Fact]
    public async Task Steps_run_in_order_and_the_log_shows_each_step()
    {
        var def = Make([new RunProcess("ipconfig.exe", "/flushdns"), new RunProcess("netsh.exe", "winsock reset")]);
        var result = await _runner.RunAsync(def, _log.Add, default);

        Assert.True(result.Success);
        Assert.Equal(["ipconfig.exe /flushdns", "netsh.exe winsock reset"], _fake.Calls);
        Assert.Contains("> ipconfig.exe /flushdns", _log);
    }

    [Fact]
    public async Task A_bad_exit_code_stops_the_run_and_names_the_tool()
    {
        _fake.ExitCodes["netsh.exe winsock reset"] = 1;
        var def = Make([new RunProcess("netsh.exe", "winsock reset"), new RunProcess("ipconfig.exe", "/flushdns")]);

        var result = await _runner.RunAsync(def, _log.Add, default);

        Assert.False(result.Success);
        Assert.Contains("netsh.exe", result.Error);
        Assert.Single(_fake.Calls);
    }

    [Fact]
    public async Task Listed_exit_codes_count_as_success()
    {
        _fake.ExitCodes["net.exe stop wuauserv"] = 2;
        var def = Make([new RunProcess("net.exe", "stop wuauserv", [0, 2])]);
        Assert.True((await _runner.RunAsync(def, _log.Add, default)).Success);
    }

    [Fact]
    public async Task Always_steps_run_after_a_failure_and_after_a_stop()
    {
        var always = new ActionStep[] { new RunProcess("net.exe", "start wuauserv", [0, 2]) };

        _fake.ExitCodes["dism.exe /x"] = 5;
        var failed = await _runner.RunAsync(Make([new RunProcess("dism.exe", "/x")], always), _log.Add, default);
        Assert.False(failed.Success);
        Assert.Contains("net.exe start wuauserv", _fake.Calls);

        _fake.Calls.Clear();
        _fake.CancelOn = k => k == "sfc.exe /scannow";
        var stopped = await _runner.RunAsync(Make([new RunProcess("sfc.exe", "/scannow")], always), _log.Add, default);
        Assert.True(stopped.Cancelled);
        Assert.Contains("net.exe start wuauserv", _fake.Calls);
    }

    [Fact]
    public async Task Tools_and_folders_outside_the_allow_lists_are_refused()
    {
        var tool = await _runner.RunAsync(Make([new RunProcess("cmd.exe", "/c calc")]), _log.Add, default);
        Assert.False(tool.Success);
        Assert.Empty(_fake.Calls);

        var folder = await _runner.RunAsync(Make([new DeleteFolder("%SystemRoot%\\System32")]), _log.Add, default);
        Assert.False(folder.Success);
        Assert.Contains("not an allowed folder", folder.Error);

        var rename = await _runner.RunAsync(Make([new RenameFolder("%SystemRoot%\\SoftwareDistribution", "..\\Evil")]), _log.Add, default);
        Assert.False(rename.Success);
    }

    [Fact]
    public async Task Update_cache_reset_renames_the_real_folders_and_replaces_old_copies()
    {
        var sd = Path.Combine(_root, "Windows", "SoftwareDistribution");
        var old = Path.Combine(_root, "Windows", "SoftwareDistribution.old");
        Directory.CreateDirectory(Path.Combine(sd, "Download"));
        File.WriteAllText(Path.Combine(sd, "Download", "new.bin"), "new");
        Directory.CreateDirectory(old);
        File.WriteAllBytes(Path.Combine(old, "stale.bin"), new byte[300]);

        var def = Make([new DeleteFolder("%SystemRoot%\\SoftwareDistribution.old"), new RenameFolder("%SystemRoot%\\SoftwareDistribution", "SoftwareDistribution.old")]);
        var result = await _runner.RunAsync(def, _log.Add, default);

        Assert.True(result.Success);
        Assert.Equal(300, result.BytesFreed);
        Assert.False(Directory.Exists(sd));
        Assert.True(File.Exists(Path.Combine(old, "Download", "new.bin")));
        Assert.False(File.Exists(Path.Combine(old, "stale.bin")));
    }

    [Fact]
    public async Task A_failed_rename_still_runs_the_always_steps()
    {
        var sd = Path.Combine(_root, "Windows", "SoftwareDistribution");
        var old = Path.Combine(_root, "Windows", "SoftwareDistribution.old");
        Directory.CreateDirectory(sd);
        Directory.CreateDirectory(old);
        File.WriteAllText(Path.Combine(old, "x"), "x");

        var def = Make([new RenameFolder("%SystemRoot%\\SoftwareDistribution", "SoftwareDistribution.old")], [new RunProcess("net.exe", "start wuauserv", [0, 2])]);
        var result = await _runner.RunAsync(def, _log.Add, default);

        Assert.False(result.Success);
        Assert.Contains("already exists", result.Error);
        Assert.Contains("net.exe start wuauserv", _fake.Calls);
    }

    [Fact]
    public async Task Arguments_get_environment_variables_expanded()
    {
        await _runner.RunAsync(Make([new RunProcess("chkdsk.exe", "%SystemDrive% /scan")]), _log.Add, default);
        Assert.Equal("chkdsk.exe C: /scan", _fake.Calls.Single());
    }

    [Fact]
    public void Embedded_repair_catalog_is_valid_and_restarts_services_in_always_steps()
    {
        var actions = CatalogLoader.LoadActions();
        Assert.True(actions.Count >= 8);
        Assert.Empty(CatalogLoader.ValidateActions(actions));

        var reset = actions.Single(a => a.Id == "repair.reset-update");
        Assert.True(reset.RestorePoint);
        Assert.Contains(reset.Always.OfType<RunProcess>(), s => s.Args == "start wuauserv");
        Assert.Contains(reset.Steps.OfType<RunProcess>(), s => s.Args == "stop wuauserv");
        Assert.True(actions.Single(a => a.Id == "repair.sfc").Steps.OfType<RunProcess>().Single().Utf16);
    }

    [Fact]
    public void Validation_rejects_unknown_tools_and_folders()
    {
        var bad = Make([new RunProcess("powershell.exe", "-c x"), new DeleteFolder("%SystemRoot%\\System32"), new RenameFolder("%TEMP%", "a\\b")]);
        var errors = CatalogLoader.ValidateActions([bad]);
        Assert.Contains(errors, e => e.Contains("powershell.exe"));
        Assert.Contains(errors, e => e.Contains("System32"));
        Assert.Contains(errors, e => e.Contains("plain folder name"));
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public async Task Real_runner_streams_a_harmless_tool_and_decodes_utf16_output()
    {
        if (!OperatingSystem.IsWindows()) return;
        var lines = new List<string>();
        var runner = new WindowsStreamingRunner();

        var code = await runner.RunAsync("ipconfig.exe", "/flushdns", false, lines.Add, default);
        Assert.Equal(0, code);
        Assert.Contains(lines, l => l.Contains("DNS", StringComparison.OrdinalIgnoreCase));

        var sfc = new List<string>();
        await runner.RunAsync("sfc.exe", "/?", true, sfc.Add, default);
        Assert.Contains(sfc, l => l.Contains("Resource Checker", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(sfc, l => l.Contains('\0'));
    }

    [Fact]
    [SupportedOSPlatform("windows")]
    public async Task Real_runner_can_be_cancelled()
    {
        if (!OperatingSystem.IsWindows()) return;
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(400));
        var runner = new WindowsStreamingRunner();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            runner.RunAsync("chkdsk.exe", "C: /scan", false, _ => { }, cts.Token));
    }
}
