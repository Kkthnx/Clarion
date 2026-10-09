using System.Diagnostics;
using Clarion.Core.Cleanup;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Profiles;

namespace Clarion.Tests;

/// <summary>Tests that try to break things: bad input, locked files, parallel use, large batches and random text.</summary>
public sealed class RobustnessTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-rob-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch (IOException) { }
    }

    private static RegistryTarget T(string name) => new(RegistryHive.CurrentUser, "Software\\Rob", name);

    private static Tweak Make(string id, params Operation[] ops) => new()
    {
        Id = id, Category = "T", Name = id, Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe, Apply = ops,
    };

    private static SetRegistryValue Set(string name, string v = "1") => new(T(name), new RegistryData(RegistryKind.DWord, v));

    [Fact]
    public void An_operation_nobody_can_run_reads_as_unavailable_and_fails_cleanly()
    {
        var engine = new TweakEngine([new RegistryHandler(new FakeRegistry())], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
        var tweak = Make("x", new SetTaskEnabled("\\A\\B", false));

        Assert.Equal(TweakState.Unavailable, engine.Detect(tweak));
        Assert.False(engine.Apply(tweak, Guid.NewGuid()).Success);
    }

    [Fact]
    public void A_journal_that_cannot_be_written_rolls_the_change_back()
    {
        var reg = new FakeRegistry();
        reg.Write(T("v"), new RegistryData(RegistryKind.DWord, "5"));
        var path = Path.Combine(_dir, "j.jsonl");
        var journal = new ChangeJournal(path);
        File.WriteAllText(path, "");
        var engine = new TweakEngine([new RegistryHandler(reg)], journal);

        using var lockIt = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        var result = engine.Apply(Make("x", Set("v", "0")), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("5", reg.Read(T("v")).Data!.Value);
    }

    [Fact]
    public void Reverting_twice_changes_nothing_the_second_time()
    {
        var reg = new FakeRegistry();
        reg.Write(T("v"), new RegistryData(RegistryKind.DWord, "5"));
        var engine = new TweakEngine([new RegistryHandler(reg)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
        var tweak = Make("x", Set("v", "0"));

        engine.Apply(tweak, Guid.NewGuid());
        Assert.True(engine.Revert(tweak, Guid.NewGuid()).Success);
        var second = engine.Revert(tweak, Guid.NewGuid());

        Assert.False(second.Success);
        Assert.Equal("5", reg.Read(T("v")).Data!.Value);
    }

    [Fact]
    public void Applying_a_hundred_and_fifty_settings_is_fast_and_every_one_can_be_reverted()
    {
        var reg = new FakeRegistry();
        var engine = new TweakEngine([new RegistryHandler(reg)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
        var tweaks = Enumerable.Range(0, 150).Select(i => Make($"t{i}", Set($"v{i}"), Set($"w{i}"))).ToList();

        var sw = Stopwatch.StartNew();
        foreach (var t in tweaks) Assert.True(engine.Apply(t, Guid.NewGuid()).Success);
        foreach (var t in tweaks) Assert.True(engine.Revert(t, Guid.NewGuid()).Success);
        sw.Stop();

        Assert.All(tweaks, t => Assert.Equal(TweakState.NotApplied, engine.Detect(t)));
        Assert.True(sw.ElapsedMilliseconds < 60000, $"Took {sw.ElapsedMilliseconds} ms");
    }

    [Fact]
    public async Task Many_threads_writing_the_journal_at_once_never_corrupt_it()
    {
        var journal = new ChangeJournal(Path.Combine(_dir, "j.jsonl"));
        var op = Set("v");
        var undo = new DeleteRegistryValue(T("v"), "Software");

        await Task.WhenAll(Enumerable.Range(0, 8).Select(t => Task.Run(() =>
        {
            for (var i = 0; i < 50; i++)
                journal.Append(new JournalEntry(Guid.NewGuid(), $"t{t}", JournalAction.Apply, DateTimeOffset.UtcNow, op, undo));
        })));

        var all = journal.ReadAll();
        Assert.Equal(400, all.Count);
        Assert.Equal(0, journal.SkippedLines);
    }

    [Fact]
    public void Five_thousand_cache_files_scan_and_clean_quickly()
    {
        // The files are made where the cleanup is allowed to work. Making them elsewhere and renaming the folder
        // failed on a CI machine, where the virus scanner can still be holding a folder with thousands of new files.
        var platform = new FakeCleanupPlatform(_dir);
        var inside = Path.Combine(platform.Local, "cache");
        Directory.CreateDirectory(inside);
        for (var d = 0; d < 50; d++)
        {
            var sub = Path.Combine(inside, $"d{d}");
            Directory.CreateDirectory(sub);
            for (var f = 0; f < 100; f++) File.WriteAllBytes(Path.Combine(sub, $"f{f}.bin"), new byte[64]);
        }
        var engine = new CleanupEngine(platform);
        var target = new CleanTarget
        {
            Id = "big", Group = "g", Name = "n", Summary = "s", Advice = "a", Benefit = "b", Risk = "r", RiskLevel = RiskLevel.Safe,
            Facts = ["a", "b"], Rules = [new FolderRule("%LOCALAPPDATA%\\cache")],
        };

        var sw = Stopwatch.StartNew();
        var scan = engine.Scan(target);
        var result = engine.Clean(target, false, false, null, default);
        sw.Stop();

        Assert.Equal(5000, scan.Files);
        Assert.Equal(5000 * 64, result.BytesFreed);
        Assert.True(sw.ElapsedMilliseconds < 15000, $"Took {sw.ElapsedMilliseconds} ms");
        Assert.Empty(Directory.GetFileSystemEntries(inside));
    }

    [Fact]
    public void Random_text_never_crashes_the_command_line_parser_or_the_setup_reader_or_the_path_guard()
    {
        var rng = new Random(12345);
        const string alphabet = "abcXYZ019 -_/\\.:*?\"'{}[],;|<>\t\u00e9\u4e2d";
        var platform = new FakeCleanupPlatform(_dir);

        for (var i = 0; i < 2000; i++)
        {
            var text = new string(Enumerable.Range(0, rng.Next(0, 80)).Select(_ => alphabet[rng.Next(alphabet.Length)]).ToArray());

            CliOptions.Parse(text.Split(' '));
            var load = ProfileFile.Parse(text);
            Assert.Null(load.Profile);
            PathGuard.IsSafe(text, platform);
        }
    }

    [Fact]
    public void Random_json_shapes_never_crash_the_setup_reader()
    {
        string[] shapes =
        [
            "[]", "null", "123", "\"text\"", "{\"format\":\"one\"}", "{\"tweaks\":{}}", "{\"format\":1,\"app\":\"Clarion\",\"tweaks\":[1,2,3]}",
            "{\"format\":1,\"app\":\"Clarion\",\"tweaks\":[null]}", "{\"format\":1,\"app\":null,\"tweaks\":[]}", "{{{{", "\u0000\u0000",
        ];
        foreach (var s in shapes) Assert.Null(ProfileFile.Parse(s).Profile);
    }

    [Fact]
    public void A_setting_with_no_facts_or_notes_still_builds_a_tooltip()
    {
        var tip = TweakTooltip.Build(Make("x", Set("v")));
        Assert.NotEmpty(tip.Notes);
        Assert.False(string.IsNullOrEmpty(tip.ToPlainText()));
    }
}
