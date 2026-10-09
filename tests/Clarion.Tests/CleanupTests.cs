using System.Diagnostics;
using Clarion.Core.Catalog;
using Clarion.Core.Cleanup;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class FakeCleanupPlatform : ICleanupPlatform
{
    public FakeCleanupPlatform(string root)
    {
        Root = root;
        Directory.CreateDirectory(Path.Combine(root, "Local"));
        Directory.CreateDirectory(Path.Combine(root, "Roaming"));
        Directory.CreateDirectory(Path.Combine(root, "LocalLow"));
        Directory.CreateDirectory(Path.Combine(root, "Windows"));
    }

    public string Root { get; }
    public string Local => Path.Combine(Root, "Local");
    public string Windows => Path.Combine(Root, "Windows");

    public IReadOnlyList<string> AllowedBases => [Local, Path.Combine(Root, "Roaming"), Path.Combine(Root, "LocalLow"), Windows];
    public IReadOnlyList<string> AllowedExact { get; set; } = [];
    public List<string> Libraries { get; } = [];
    public List<string> WowFolders { get; } = [];
    public HashSet<string> Running { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Queued { get; } = [];
    public List<string> EventLogs { get; } = ["Application", "System", "Security"];
    public List<string> ClearedLogs { get; } = [];
    public Dictionary<string, string> LogBackups { get; } = [];
    public HashSet<string> LockedFiles { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> LogsThatCannotBeSaved { get; } = [];
    public string DataFolder => Path.Combine(Root, "Data");
    public bool IsLocked(string file) => LockedFiles.Contains(file);
    public bool BinEmptied { get; private set; }
    public bool QueueWorks { get; set; } = true;

    public string Expand(string template) => template
        .Replace("%LOCALAPPDATA%", Local, StringComparison.OrdinalIgnoreCase)
        .Replace("%APPDATA%", Path.Combine(Root, "Roaming"), StringComparison.OrdinalIgnoreCase)
        .Replace("%LOCALLOW%", Path.Combine(Root, "LocalLow"), StringComparison.OrdinalIgnoreCase)
        .Replace("%SystemRoot%", Windows, StringComparison.OrdinalIgnoreCase)
        .Replace("%TEMP%", Path.Combine(Local, "Temp"), StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<string> SteamLibraries() => Libraries;
    public IReadOnlyList<string> WowVersionFolders() => WowFolders;
    public IReadOnlySet<string> RunningProcesses() => Running;
    public bool QueueDeleteAtRestart(string file) { if (QueueWorks) Queued.Add(file); return QueueWorks; }
    public long RecycleBinBytes() => 1234;
    public void EmptyRecycleBin() => BinEmptied = true;
    public IReadOnlyList<string> EventLogNames() => EventLogs;
    public void ClearEventLog(string name, string backupFile)
    {
        if (LogsThatCannotBeSaved.Contains(name)) throw new IOException("The copy could not be saved.");
        LogBackups[name] = backupFile;
        ClearedLogs.Add(name);
    }
}

public sealed class CleanupTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "clarion-clean-" + Guid.NewGuid().ToString("N"));
    private readonly FakeCleanupPlatform _p;
    private readonly CleanupEngine _engine;

    public CleanupTests()
    {
        Directory.CreateDirectory(_root);
        _p = new FakeCleanupPlatform(_root);
        _engine = new CleanupEngine(_p);
    }

    public void Dispose()
    {
        try { Directory.Delete(_root, true); } catch (IOException) { }
    }

    private static CleanTarget Target(string id, int age = 0, bool queue = false, string[]? procs = null, params CleanRule[] rules) => new()
    {
        Id = id, Group = "g", Name = id, Summary = "s", Advice = "a", Benefit = "b", Risk = "r", RiskLevel = RiskLevel.Safe,
        Facts = ["one", "two"], MinAgeHours = age, QueueLockedAtRestart = queue, Processes = procs ?? [], Rules = rules,
    };

    private string Make(string relative, int bytes = 100, DateTime? written = null)
    {
        var path = Path.Combine(_p.Local, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, new byte[bytes]);
        if (written is not null) File.SetLastWriteTimeUtc(path, written.Value);
        return path;
    }

    [Fact]
    public void Scan_reports_size_and_file_count_per_folder()
    {
        Make("NVIDIA\\DXCache\\a.bin", 1000);
        Make("NVIDIA\\DXCache\\sub\\b.bin", 500);
        var t = Target("n", rules: [new FolderRule("%LOCALAPPDATA%\\NVIDIA\\DXCache"), new FolderRule("%LOCALAPPDATA%\\NVIDIA\\GLCache")]);

        var scan = _engine.Scan(t);

        Assert.Equal(1500, scan.Bytes);
        Assert.Equal(2, scan.Files);
        Assert.True(scan.Folders[0].Exists);
        Assert.False(scan.Folders[1].Exists);
        Assert.True(scan.Present);
    }

    [Fact]
    public void Clean_deletes_files_keeps_the_root_folder_and_counts_only_real_deletes()
    {
        var a = Make("Cache\\a.bin", 1000);
        Make("Cache\\deep\\b.bin", 500);
        var t = Target("c", rules: [new FolderRule("%LOCALAPPDATA%\\Cache")]);

        var result = _engine.Clean(t, false, false, null, default);

        Assert.Equal(1500, result.BytesFreed);
        Assert.Equal(2, result.FilesDeleted);
        Assert.False(File.Exists(a));
        Assert.True(Directory.Exists(Path.Combine(_p.Local, "Cache")));
        Assert.False(Directory.Exists(Path.Combine(_p.Local, "Cache", "deep")));
    }

    [Fact]
    public void A_locked_file_is_left_alone_and_never_counted_as_freed()
    {
        var free = Make("Cache\\free.bin", 100);
        var held = Make("Cache\\held.bin", 700);
        var t = Target("c", rules: [new FolderRule("%LOCALAPPDATA%\\Cache")]);

        using var hold = new FileStream(held, FileMode.Open, FileAccess.Read, FileShare.None);
        var result = _engine.Clean(t, false, false, null, default);

        Assert.Equal(100, result.BytesFreed);
        Assert.Equal(1, result.FilesLocked);
        Assert.False(File.Exists(free));
        Assert.True(File.Exists(held));
    }

    [Fact]
    public void Locked_files_are_queued_for_restart_only_when_the_row_allows_it()
    {
        var held = Make("Cache\\held.bin", 700);
        using var hold = new FileStream(held, FileMode.Open, FileAccess.Read, FileShare.None);

        var allowed = Target("c", queue: true, rules: [new FolderRule("%LOCALAPPDATA%\\Cache")]);
        var r1 = _engine.Clean(allowed, queueLocked: true, preview: false, null, default);
        Assert.Equal(1, r1.QueuedForRestart);
        Assert.Contains(held, _p.Queued);

        _p.Queued.Clear();
        var denied = Target("c", queue: false, rules: [new FolderRule("%LOCALAPPDATA%\\Cache")]);
        var r2 = _engine.Clean(denied, queueLocked: true, preview: false, null, default);
        Assert.Equal(0, r2.QueuedForRestart);
        Assert.Empty(_p.Queued);
    }

    [Fact]
    public void Age_rule_keeps_recent_files()
    {
        var old = Make("T\\old.tmp", 100, DateTime.UtcNow.AddDays(-3));
        var fresh = Make("T\\fresh.tmp", 100, DateTime.UtcNow);
        var t = Target("t", age: 24, rules: [new FolderRule("%LOCALAPPDATA%\\T")]);

        Assert.Equal(100, _engine.Scan(t).Bytes);
        var result = _engine.Clean(t, false, false, null, default);

        Assert.Equal(100, result.BytesFreed);
        Assert.False(File.Exists(old));
        Assert.True(File.Exists(fresh));
    }

    [Fact]
    public void Preview_measures_but_deletes_nothing()
    {
        var f = Make("Cache\\a.bin", 400);
        var t = Target("c", rules: [new FolderRule("%LOCALAPPDATA%\\Cache")]);

        var result = _engine.Clean(t, false, preview: true, null, default);

        Assert.Equal(400, result.BytesFreed);
        Assert.True(File.Exists(f));
    }

    [Fact]
    public void File_pattern_rule_only_touches_matching_files()
    {
        var thumb = Make("Explorer\\thumbcache_96.db", 100);
        var keep = Make("Explorer\\iconcache_96.db", 100);
        var t = Target("th", rules: [new FilePatternRule("%LOCALAPPDATA%\\Explorer", "thumbcache_*.db")]);

        _engine.Clean(t, false, false, null, default);

        Assert.False(File.Exists(thumb));
        Assert.True(File.Exists(keep));
    }

    [Fact]
    public void Folder_wildcard_in_the_last_segment_matches_numbered_folders()
    {
        var a = Make("Epic\\webcache\\x.bin");
        var b = Make("Epic\\webcache_4147\\y.bin");
        var other = Make("Epic\\Config\\z.bin");
        var t = Target("e", rules: [new FolderRule("%LOCALAPPDATA%\\Epic\\webcache*")]);

        _engine.Clean(t, false, false, null, default);

        Assert.False(File.Exists(a));
        Assert.False(File.Exists(b));
        Assert.True(File.Exists(other));
    }

    [Fact]
    public void A_running_app_makes_the_row_skip_with_a_note()
    {
        var f = Make("discord\\Cache\\a.bin");
        _p.Running.Add("Discord");
        var t = Target("d", procs: ["Discord"], rules: [new FolderRule("%LOCALAPPDATA%\\discord\\Cache")]);

        var scan = _engine.Scan(t);
        var result = _engine.Clean(t, false, false, null, default);

        Assert.Contains("Discord", scan.RunningApps);
        Assert.True(result.Skipped);
        Assert.True(File.Exists(f));
        Assert.Contains("Discord", result.Notes[0]);
    }

    [Fact]
    public void Steam_rule_cleans_the_shadercache_of_every_library_and_nothing_else()
    {
        var lib1 = Path.Combine(_root, "lib1");
        var lib2 = Path.Combine(_root, "lib2");
        foreach (var lib in new[] { lib1, lib2 })
        {
            Directory.CreateDirectory(Path.Combine(lib, "steamapps", "shadercache", "123"));
            File.WriteAllBytes(Path.Combine(lib, "steamapps", "shadercache", "123", "s.bin"), new byte[200]);
            Directory.CreateDirectory(Path.Combine(lib, "steamapps", "common", "Game"));
            File.WriteAllBytes(Path.Combine(lib, "steamapps", "common", "Game", "game.exe"), new byte[50]);
        }
        _p.Libraries.AddRange([lib1, lib2]);
        var t = Target("s", rules: [new SteamShaderRule()]);

        var result = _engine.Clean(t, false, false, null, default);

        Assert.Equal(400, result.BytesFreed);
        Assert.True(File.Exists(Path.Combine(lib1, "steamapps", "common", "Game", "game.exe")));
        Assert.True(File.Exists(Path.Combine(lib2, "steamapps", "common", "Game", "game.exe")));
    }

    [Fact]
    public void Wow_rule_only_cleans_the_Cache_folder_of_a_real_install()
    {
        var wow = Path.Combine(_root, "WoW", "_retail_");
        Directory.CreateDirectory(Path.Combine(wow, "Cache"));
        Directory.CreateDirectory(Path.Combine(wow, "WTF"));
        Directory.CreateDirectory(Path.Combine(wow, "Interface", "AddOns"));
        File.WriteAllBytes(Path.Combine(wow, "Wow.exe"), new byte[10]);
        File.WriteAllBytes(Path.Combine(wow, "Cache", "c.bin"), new byte[300]);
        File.WriteAllBytes(Path.Combine(wow, "WTF", "Config.wtf"), new byte[20]);
        _p.WowFolders.Add(wow);

        var notWow = Path.Combine(_root, "NotWow", "_retail_");
        Directory.CreateDirectory(Path.Combine(notWow, "Cache"));
        File.WriteAllBytes(Path.Combine(notWow, "Cache", "keep.bin"), new byte[5]);
        _p.WowFolders.Add(notWow);

        var result = _engine.Clean(Target("w", rules: [new WowCacheRule()]), false, false, null, default);

        Assert.Equal(300, result.BytesFreed);
        Assert.True(File.Exists(Path.Combine(wow, "WTF", "Config.wtf")));
        Assert.True(File.Exists(Path.Combine(wow, "Wow.exe")));
        Assert.True(File.Exists(Path.Combine(notWow, "Cache", "keep.bin")));
    }

    [Fact]
    public void Links_inside_a_cache_are_removed_without_being_followed()
    {
        var outside = Path.Combine(_root, "outside");
        Directory.CreateDirectory(outside);
        var precious = Path.Combine(outside, "precious.txt");
        File.WriteAllText(precious, "keep me");
        Make("Cache\\a.bin", 100);

        var link = Path.Combine(_p.Local, "Cache", "linked");
        var psi = new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{link}\" \"{outside}\"") { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        using (var proc = Process.Start(psi)!) { proc.WaitForExit(); if (proc.ExitCode != 0) return; }

        var t = Target("c", rules: [new FolderRule("%LOCALAPPDATA%\\Cache")]);
        _engine.Clean(t, false, false, null, default);

        Assert.True(File.Exists(precious));
        Assert.False(Directory.Exists(link));
    }

    [Fact]
    public void A_cache_folder_that_is_itself_a_link_is_refused()
    {
        var outside = Path.Combine(_root, "outside2");
        Directory.CreateDirectory(outside);
        var precious = Path.Combine(outside, "precious.txt");
        File.WriteAllText(precious, "keep me");
        var link = Path.Combine(_p.Local, "SwappedCache");
        var psi = new ProcessStartInfo("cmd.exe", $"/c mklink /J \"{link}\" \"{outside}\"") { CreateNoWindow = true, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
        using (var proc = Process.Start(psi)!) { proc.WaitForExit(); if (proc.ExitCode != 0) return; }

        var result = _engine.Clean(Target("c", rules: [new FolderRule("%LOCALAPPDATA%\\SwappedCache")]), false, false, null, default);

        Assert.True(File.Exists(precious));
        Assert.Equal(0, result.FilesDeleted);
        Assert.Contains(result.Notes, n => n.Contains("allow list or is a link"));
    }

    [Theory]
    [InlineData("C:\\")]
    [InlineData("C:")]
    [InlineData("")]
    public void Drive_roots_and_empty_paths_are_never_safe(string path)
    {
        Assert.False(PathGuard.IsSafe(path, _p));
    }

    [Fact]
    public void Paths_outside_the_allowed_bases_or_with_dot_dot_are_refused()
    {
        Assert.False(PathGuard.IsSafe(Path.Combine(_root, "elsewhere", "x"), _p));
        Assert.False(PathGuard.IsSafe(Path.Combine(_p.Local, "a", "..", "..", "x"), _p));
        Assert.False(PathGuard.IsSafe(_p.Local, _p));
        Assert.True(PathGuard.IsSafe(Path.Combine(_p.Local, "ok"), _p));
    }

    [Fact]
    public void Cancelling_stops_the_clean_between_files()
    {
        for (var i = 0; i < 20; i++) Make($"Cache\\f{i}.bin", 10);
        var cts = new CancellationTokenSource();
        var seen = 0;
        var t = Target("c", rules: [new FolderRule("%LOCALAPPDATA%\\Cache")]);

        Assert.Throws<OperationCanceledException>(() => _engine.Clean(t, false, false, _ => { if (++seen == 5) cts.Cancel(); }, cts.Token));
        Assert.InRange(Directory.GetFiles(Path.Combine(_p.Local, "Cache")).Length, 1, 19);
    }

    [Fact]
    public void Clearing_event_logs_is_expert_only_and_never_suggested()
    {
        var rows = CatalogLoader.LoadCleanup().Where(t => t.Rules.Any(r => r is EventLogsRule)).ToList();

        Assert.NotEmpty(rows);
        Assert.All(rows, t =>
        {
            Assert.True(t.ExpertOnly, t.Id);
            Assert.Equal(Recommendation.Avoid, t.Recommendation);
            Assert.False(t.DefaultOn, t.Id);
        });
    }

    [Fact]
    public void Each_event_log_is_saved_as_an_evtx_copy_in_the_clarion_folder_before_it_is_cleared()
    {
        var logs = Target("ev", rules: [new EventLogsRule()]);

        var result = _engine.Clean(logs, false, false, null, CancellationToken.None);

        Assert.Equal(["Application", "System"], _p.ClearedLogs);
        Assert.All(_p.LogBackups.Values, path =>
        {
            Assert.StartsWith(Path.Combine(_p.DataFolder, "EventLogBackups"), path);
            Assert.EndsWith(".evtx", path);
        });
        Assert.Equal(2, _p.LogBackups.Values.Distinct().Count());
        Assert.Contains(result.Notes, n => n.Contains("EventLogBackups"));
    }

    [Fact]
    public void A_log_whose_copy_cannot_be_saved_is_not_cleared()
    {
        _p.LogsThatCannotBeSaved.Add("System");
        var logs = Target("ev", rules: [new EventLogsRule()]);

        var result = _engine.Clean(logs, false, false, null, CancellationToken.None);

        Assert.Equal(["Application"], _p.ClearedLogs);
        Assert.Equal(1, result.FilesLocked);
    }

    [Fact]
    public void A_preview_clears_nothing_and_saves_nothing()
    {
        var result = _engine.Clean(Target("ev", rules: [new EventLogsRule()]), false, true, null, CancellationToken.None);

        Assert.Empty(_p.ClearedLogs);
        Assert.Empty(_p.LogBackups);
        Assert.Equal(0, result.FilesDeleted);
    }

    [Fact]
    public void A_preview_lists_the_files_in_use_that_a_real_clean_would_queue_and_does_not_count_them_as_freed()
    {
        var free = Make("cache\\free.bin", 100);
        var held = Make("cache\\held.bin", 300);
        _p.LockedFiles.Add(held);
        var target = Target("c", queue: true, rules: new FolderRule("%LOCALAPPDATA%\\cache"));

        var preview = _engine.Clean(target, true, true, null, CancellationToken.None);

        Assert.Equal([held], preview.QueuedFiles);
        Assert.Equal(100, preview.BytesFreed);
        Assert.Empty(_p.Queued);
        Assert.True(File.Exists(held) && File.Exists(free));
        Assert.Contains(preview.Notes, n => n.Contains("A real clean would"));
    }

    [Fact]
    public void A_real_clean_reports_exactly_which_files_were_set_for_restart()
    {
        var held = Make("cache\\held.bin");
        var target = Target("c", queue: true, rules: new FolderRule("%LOCALAPPDATA%\\cache"));
        using var open = new FileStream(held, FileMode.Open, FileAccess.Read, FileShare.None);

        var result = _engine.Clean(target, true, false, null, CancellationToken.None);

        Assert.Equal([held], result.QueuedFiles);
        Assert.Equal([held], _p.Queued);
    }

    [Fact]
    public void Event_logs_never_include_Security_and_recycle_bin_is_emptied_through_the_platform()
    {
        var logs = Target("ev", rules: [new EventLogsRule()]);
        _engine.Clean(logs, false, false, null, default);
        Assert.Equal(["Application", "System"], _p.ClearedLogs);

        var bin = Target("bin", rules: [new RecycleBinRule()]);
        var result = _engine.Clean(bin, false, false, null, default);
        Assert.True(_p.BinEmptied);
        Assert.Equal(1234, result.BytesFreed);
    }

    [Fact]
    public void Embedded_cleanup_catalog_loads_and_is_valid()
    {
        var targets = CatalogLoader.LoadCleanup();
        Assert.True(targets.Count >= 20);
        Assert.Empty(CatalogLoader.ValidateCleanup(targets));
        Assert.All(targets.Where(t => t.Irreversible), t => Assert.False(t.DefaultOn));
        Assert.Contains(targets, t => t.Id == "shaders.nvidia" && t.QueueLockedAtRestart);
        Assert.DoesNotContain(targets.SelectMany(t => t.Rules).OfType<FolderRule>(), r => r.Path.Contains("Security", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validation_rejects_traversal_unknown_roots_and_bad_patterns()
    {
        var bad = new[]
        {
            Target("a", rules: [new FolderRule("%LOCALAPPDATA%\\..\\Windows")]),
            Target("b", rules: [new FolderRule("C:\\Users")]),
            Target("c", rules: [new FolderRule("%SystemRoot%")]),
            Target("d", rules: [new FilePatternRule("%LOCALAPPDATA%\\x", "..\\*.dll")]),
        };
        var errors = CatalogLoader.ValidateCleanup(bad);
        Assert.Contains(errors, e => e.StartsWith("a:", StringComparison.Ordinal) && e.Contains(".."));
        Assert.Contains(errors, e => e.StartsWith("b:", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.StartsWith("c:", StringComparison.Ordinal));
        Assert.Contains(errors, e => e.StartsWith("d:", StringComparison.Ordinal));
    }
}
