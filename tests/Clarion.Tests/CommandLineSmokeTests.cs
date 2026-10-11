using System.Diagnostics;
using System.Runtime.Versioning;
using Clarion.Core.Catalog;
using Clarion.Engine;
using Xunit.Abstractions;

namespace Clarion.Tests;

/// <summary>
/// Starts the real program from the command line, the way a script or the scheduled monthly check does. The runner behind these commands
/// builds the real runtime, so it cannot be tested with fakes, and a build whose command line is broken is a broken release.
/// Needs the app to be built first (CI does that). On a machine where it has not been built the tests do nothing, unless CI is set,
/// where a missing program is a failure.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class CommandLineSmokeTests(ITestOutputHelper output) : IDisposable
{
    private readonly string _data = Path.Combine(Path.GetTempPath(), "clarion-cli-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_data)) Directory.Delete(_data, true);
    }

    private static string? FindExe()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) dir = dir.Parent;
        if (dir is null) return null;
        var bin = Path.Combine(dir.FullName, "src", "Clarion.App", "bin");
        if (!Directory.Exists(bin)) return null;
        return Directory.EnumerateFiles(bin, "Clarion.exe", SearchOption.AllDirectories)
            .Where(f => f.Contains($"{Path.DirectorySeparatorChar}x64{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(File.GetLastWriteTimeUtc)
            .FirstOrDefault();
    }

    private (int Code, string Output)? Run(params string[] args)
    {
        var exe = FindExe();
        if (exe is null)
        {
            Assert.False(Environment.GetEnvironmentVariable("CI") == "true", "CI is set but Clarion.exe was not built.");
            output.WriteLine("Clarion.exe is not built here, so this test did nothing.");
            return null;
        }

        var info = new ProcessStartInfo(exe) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        foreach (var a in args) info.ArgumentList.Add(a);
        info.Environment["CLARION_DATA_DIR"] = _data;
        info.Environment["CLARION_NO_ELEVATE"] = "1";

        using var process = Process.Start(info)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        Assert.True(process.WaitForExit(120_000), $"Clarion.exe {string.Join(' ', args)} did not finish.");
        var text = stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult();
        output.WriteLine($"> {string.Join(' ', args)}  exit {process.ExitCode}\n{text}");
        return (process.ExitCode, text);
    }

    [Fact]
    public void Version_prints_the_version_and_succeeds()
    {
        if (Run("--version") is not var (code, text)) return;
        Assert.Equal(0, code);
        Assert.Matches(@"^\.Clarion \d+\.\d+\.\d+", text.Trim());
    }

    [Fact]
    public void Help_lists_every_command_and_an_unknown_one_exits_with_2()
    {
        if (Run("--help") is not var (code, text)) return;
        Assert.Equal(0, code);
        foreach (var command in new[] { "--apply-preset", "--clean", "--verify", "--history", "--list-presets", "--list-settings" })
            Assert.Contains(command, text);

        Assert.Equal(2, Run("--no-such-command")!.Value.Code);
    }

    [Fact]
    public void The_list_commands_agree_with_the_catalog()
    {
        if (Run("--list-presets") is not var (presetCode, presets)) return;
        Assert.Equal(0, presetCode);
        foreach (var (name, ids) in CatalogLoader.LoadPresets()) Assert.Contains($"{name} ({ids.Count} settings)", presets);

        var settings = Run("--list-settings")!.Value;
        Assert.Equal(0, settings.Code);
        var catalog = CatalogLoader.LoadEmbedded();
        Assert.All(catalog.Take(25), t => Assert.Contains(t.Id, settings.Output));

        var cleanups = Run("--list-cleanups")!.Value;
        Assert.Equal(0, cleanups.Code);
        Assert.Contains(CatalogLoader.LoadCleanup().First().Id, cleanups.Output);
    }

    [Fact]
    public void The_read_only_commands_work_without_administrator_rights_and_on_an_empty_history()
    {
        if (Run("--history") is not var (historyCode, _)) return;
        Assert.Equal(0, historyCode);

        Assert.Equal(0, Run("--verify")!.Value.Code);
        Assert.Equal(0, Run("--list-symptoms")!.Value.Code);
        Assert.Equal(2, Run("--what-broke", "no-such-symptom")!.Value.Code);
    }

    [Fact]
    public void A_preset_preview_lists_what_it_would_do_and_changes_nothing()
    {
        if (!WindowsMachine.IsElevated()) return;
        if (Run("--apply-preset", "standard", "--preview") is not var (code, text)) return;
        Assert.Equal(0, code);
        Assert.Matches(@"\d+ to turn on, \d+ already on", text);
        Assert.DoesNotContain("turning on:", text);

        var journal = Path.Combine(_data, "journal.jsonl");
        Assert.True(!File.Exists(journal) || new FileInfo(journal).Length == 0, "A preview must not write to the history.");
    }

    [Fact]
    public void A_bad_preset_name_is_refused_with_2()
    {
        if (!WindowsMachine.IsElevated()) return;
        if (Run("--apply-preset", "no-such-preset", "--preview") is not var (code, text)) return;
        Assert.Equal(2, code);
        Assert.Contains("no preset called", text);
    }
}
