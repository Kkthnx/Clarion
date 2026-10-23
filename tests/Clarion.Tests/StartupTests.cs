using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Core.Startup;

namespace Clarion.Tests;

public sealed class FakeStartupSource : IStartupSource
{
    public List<RawStartupItem> Raw { get; } = [];
    public Dictionary<(StartupOrigin, string), byte[]> Marks { get; } = [];
    public HashSet<string> Missing { get; } = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<RawStartupItem> Items() => Raw;

    public byte[]? Approval(StartupOrigin origin, string name) => Marks.GetValueOrDefault((origin, name));

    public StartupProgram Describe(string command)
    {
        var path = StartupCommand.ProgramPath(command);
        return new StartupProgram(path, path is not null && !Missing.Contains(path), "Maker", "Does a thing");
    }
}

public sealed class StartupTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-startup-" + Guid.NewGuid().ToString("N"));
    private readonly FakeRegistry _reg = new();
    private readonly ChangeJournal _journal;
    private readonly TweakEngine _engine;

    public StartupTests()
    {
        _journal = new ChangeJournal(Path.Combine(_dir, "journal.jsonl"));
        _engine = new TweakEngine([new RegistryHandler(_reg), new StartupHandler(_reg)], _journal);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static StartupEntry Entry(StartupOrigin origin, string name, bool enabled = true) =>
        new(origin, name, "\"C:\\Program Files\\X\\x.exe\" -min", enabled, null, new StartupProgram("C:\\Program Files\\X\\x.exe", true, "", ""), null);

    private static byte[] Bytes(string hex) => Convert.FromHexString(hex);

    private RegistryTarget MarkOf(StartupEntry e) =>
        new(e.NeedsAdmin ? RegistryHive.LocalMachine : RegistryHive.CurrentUser, StartupApproval.KeyOf(e.Origin), e.Name);

    // ---- The mark Windows keeps. The bytes below are the ones read from a real PC. ----

    [Theory]
    [InlineData("020000000000000000000000", true)]
    [InlineData("060000000000000000000000", true)]
    [InlineData("030000002BAE0723330FDC01", false)]
    [InlineData("070000000000000000000000", false)]
    public void An_even_first_byte_is_on_and_an_odd_one_is_off(string hex, bool enabled) =>
        Assert.Equal(enabled, StartupApproval.IsEnabled(Bytes(hex)));

    [Fact]
    public void No_mark_and_an_empty_mark_count_as_on()
    {
        Assert.True(StartupApproval.IsEnabled(null));
        Assert.True(StartupApproval.IsEnabled([]));
    }

    [Fact]
    public void The_time_an_entry_was_turned_off_is_read_from_the_last_eight_bytes()
    {
        var at = StartupApproval.DisabledAt(Bytes("030000002BAE0723330FDC01"));
        Assert.Equal(2025, at?.Year);
        Assert.Null(StartupApproval.DisabledAt(Bytes("020000000000000000000000")));
    }

    [Fact]
    public void An_off_mark_made_here_reads_back_as_off_at_that_time()
    {
        var when = new DateTimeOffset(2026, 10, 23, 12, 30, 0, TimeSpan.Zero);
        var mark = Bytes(StartupApproval.DisabledHex(when));

        Assert.Equal(12, mark.Length);
        Assert.False(StartupApproval.IsEnabled(mark));
        Assert.Equal(when.ToUniversalTime(), StartupApproval.DisabledAt(mark)?.ToUniversalTime());
    }

    [Theory]
    [InlineData(StartupOrigin.UserRun, "Run")]
    [InlineData(StartupOrigin.MachineRun, "Run")]
    [InlineData(StartupOrigin.MachineRun32, "Run32")]
    [InlineData(StartupOrigin.UserFolder, "StartupFolder")]
    [InlineData(StartupOrigin.MachineFolder, "StartupFolder")]
    public void Each_place_has_its_own_key_for_the_mark(StartupOrigin origin, string leaf) =>
        Assert.EndsWith("\\Explorer\\StartupApproved\\" + leaf, StartupApproval.KeyOf(origin), StringComparison.Ordinal);

    // ---- Which program a command starts. ----

    [Theory]
    [InlineData("\"C:\\Program Files\\Mem Reduct\\memreduct.exe\" -minimized", "C:\\Program Files\\Mem Reduct\\memreduct.exe")]
    [InlineData("C:\\WINDOWS\\system32\\SecurityHealthSystray.exe", "C:\\WINDOWS\\system32\\SecurityHealthSystray.exe")]
    [InlineData("C:\\Program Files (x86)\\Skillbrains\\lightshot\\Lightshot.exe /silent", "C:\\Program Files (x86)\\Skillbrains\\lightshot\\Lightshot.exe")]
    [InlineData("rundll32.exe shell32.dll,Something", "rundll32.exe")]
    [InlineData("tool.cmd", "tool.cmd")]
    public void The_program_is_found_in_quoted_and_unquoted_commands(string command, string expected) =>
        Assert.Equal(expected, StartupCommand.ProgramPath(command));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\"unterminated")]
    public void A_command_with_no_program_gives_none(string command) =>
        Assert.Null(StartupCommand.ProgramPath(command));

    // ---- The list. ----

    [Fact]
    public void The_list_merges_what_Windows_starts_with_the_marks_and_puts_what_runs_first()
    {
        var source = new FakeStartupSource();
        source.Raw.Add(new RawStartupItem(StartupOrigin.UserRun, "Zed", "\"C:\\a\\zed.exe\""));
        source.Raw.Add(new RawStartupItem(StartupOrigin.UserRun, "Off one", "\"C:\\a\\off.exe\""));
        source.Raw.Add(new RawStartupItem(StartupOrigin.MachineRun, "Alpha", "\"C:\\a\\alpha.exe\""));
        source.Marks[(StartupOrigin.UserRun, "Off one")] = Bytes("030000002BAE0723330FDC01");
        source.Marks[(StartupOrigin.MachineRun, "Alpha")] = Bytes("020000000000000000000000");

        var list = StartupList.Build(source);

        Assert.Equal(["Alpha", "Zed", "Off one"], list.Select(e => e.Name).ToList());
        Assert.False(list[2].Enabled);
        Assert.NotNull(list[2].DisabledAt);
        Assert.True(list[0].NeedsAdmin);
        Assert.False(list[1].NeedsAdmin);
    }

    [Fact]
    public void The_same_name_in_two_places_is_two_entries_and_a_repeat_in_one_place_is_one()
    {
        var source = new FakeStartupSource();
        source.Raw.Add(new RawStartupItem(StartupOrigin.UserRun, "App", "\"C:\\a\\app.exe\""));
        source.Raw.Add(new RawStartupItem(StartupOrigin.MachineRun, "App", "\"C:\\a\\app.exe\""));
        source.Raw.Add(new RawStartupItem(StartupOrigin.UserRun, "app", "\"C:\\a\\app.exe\""));

        Assert.Equal(2, StartupList.Build(source).Count);
    }

    [Fact]
    public void Windows_Security_is_not_offered_and_says_why()
    {
        var source = new FakeStartupSource();
        source.Raw.Add(new RawStartupItem(StartupOrigin.MachineRun, "SecurityHealth", "C:\\WINDOWS\\system32\\SecurityHealthSystray.exe"));
        source.Raw.Add(new RawStartupItem(StartupOrigin.MachineRun, "Renamed", "C:\\WINDOWS\\system32\\SecurityHealthSystray.exe"));

        var list = StartupList.Build(source);

        Assert.All(list, e => Assert.True(e.IsProtected));
        Assert.All(list, e => Assert.Contains("Windows Security", e.Warning, StringComparison.Ordinal));
    }

    [Fact]
    public void An_entry_whose_program_is_gone_says_so_but_a_folder_item_is_not_judged()
    {
        var source = new FakeStartupSource();
        source.Missing.Add("C:\\gone\\gone.exe");
        source.Raw.Add(new RawStartupItem(StartupOrigin.UserRun, "Gone", "\"C:\\gone\\gone.exe\""));
        source.Raw.Add(new RawStartupItem(StartupOrigin.UserFolder, "Link.lnk", "C:\\gone\\gone.exe"));

        var list = StartupList.Build(source).ToDictionary(e => e.Name);

        Assert.Contains("not there any more", list["Gone"].Warning, StringComparison.Ordinal);
        Assert.Null(list["Link.lnk"].Warning);
    }

    // ---- Ids, so a change can be named and turned back on from the history alone. ----

    [Theory]
    [InlineData(StartupOrigin.UserRun, "Spotify")]
    [InlineData(StartupOrigin.MachineRun32, "Corsair iCUE5 Software")]
    [InlineData(StartupOrigin.UserFolder, "ShareX.lnk")]
    [InlineData(StartupOrigin.MachineRun, "com.electron.Guilded")]
    public void An_id_gives_back_the_place_and_the_name_even_when_the_name_has_dots(StartupOrigin origin, string name)
    {
        var id = StartupTweaks.IdOf(origin, name);

        Assert.True(StartupTweaks.TryParse(id, out var o, out var n));
        Assert.Equal((origin, name), (o, n));
        Assert.True(StartupTweaks.IsStartupId(id));
        Assert.Equal($"Stop {name} starting with Windows", StartupTweaks.NameOf(id));
    }

    [Theory]
    [InlineData("privacy.telemetry-tasks")]
    [InlineData("startup.nowhere.X")]
    [InlineData("startup.user-run.")]
    [InlineData("startup.user-run")]
    public void An_id_that_is_not_a_startup_change_is_not_one(string id)
    {
        Assert.False(StartupTweaks.IsStartupId(id));
        Assert.Equal(id, StartupTweaks.NameOf(id));
        Assert.Null(StartupTweaks.EntryFromId(id));
    }

    // ---- Turning off and on through the engine, with the registry faked. ----

    [Fact]
    public void Turning_an_entry_off_writes_the_off_mark_and_nothing_else()
    {
        var entry = Entry(StartupOrigin.UserRun, "Spotify");
        _reg.Write(MarkOf(entry), new RegistryData(RegistryKind.Binary, StartupApproval.EnabledHex));

        var result = _engine.Apply(StartupTweaks.TurnOff(entry), Guid.NewGuid());

        Assert.True(result.Success);
        Assert.False(StartupApproval.IsEnabled(Bytes(_reg.Read(MarkOf(entry)).Data!.Value)));
    }

    [Fact]
    public void Turning_it_back_on_puts_back_the_exact_mark_that_was_there()
    {
        var entry = Entry(StartupOrigin.MachineRun, "Logi Tune");
        _reg.Write(MarkOf(entry), new RegistryData(RegistryKind.Binary, "060000000000000000000000"));
        var tweak = StartupTweaks.TurnOff(entry);

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.Equal("060000000000000000000000", _reg.Read(MarkOf(entry)).Data!.Value);
        Assert.Empty(_journal.OutstandingFor(entry.Id));
    }

    [Fact]
    public void An_entry_that_had_no_mark_goes_back_to_having_none()
    {
        var entry = Entry(StartupOrigin.UserRun, "Fresh");
        var tweak = StartupTweaks.TurnOff(entry);

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_reg.Read(MarkOf(entry)).Exists);
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.False(_reg.Read(MarkOf(entry)).Exists);
    }

    [Fact]
    public void An_entry_something_else_turned_off_is_turned_on_with_the_on_mark()
    {
        var entry = Entry(StartupOrigin.UserRun, "ByTaskManager", enabled: false);
        _reg.Write(MarkOf(entry), new RegistryData(RegistryKind.Binary, "030000002BAE0723330FDC01"));

        // Nothing of .Clarion's is recorded for it, so Revert uses the on mark in Undo.
        Assert.True(_engine.Revert(StartupTweaks.TurnOff(entry), Guid.NewGuid()).Success);

        Assert.True(StartupApproval.IsEnabled(Bytes(_reg.Read(MarkOf(entry)).Data!.Value)));
    }

    // ---- Checking that a change still holds. ----

    [Fact]
    public void A_turned_off_entry_still_holds_when_a_program_rewrites_the_mark_with_another_time()
    {
        var entry = Entry(StartupOrigin.UserRun, "Chatty");
        var tweak = StartupTweaks.TurnOff(entry);
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);

        _reg.Write(MarkOf(entry), new RegistryData(RegistryKind.Binary, "030000002BAE0723330FDC01"));

        Assert.Empty(_engine.CheckRecorded(tweak).Broken);
    }

    [Fact]
    public void An_entry_that_a_program_turned_back_on_is_reported_in_plain_words()
    {
        var entry = Entry(StartupOrigin.UserRun, "Spotify");
        var tweak = StartupTweaks.TurnOff(entry);
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);

        _reg.Write(MarkOf(entry), new RegistryData(RegistryKind.Binary, StartupApproval.EnabledHex));

        var broken = Assert.Single(_engine.CheckRecorded(tweak).Broken);
        Assert.Equal("Spotify is set to start with Windows again. A program can turn its own entry back on.", broken.Reason);
    }

    [Fact]
    public void An_entry_whose_mark_was_deleted_counts_as_turned_on_again()
    {
        var entry = Entry(StartupOrigin.UserRun, "Gone");
        var tweak = StartupTweaks.TurnOff(entry);
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);

        _reg.DeleteValue(MarkOf(entry));

        Assert.Single(_engine.CheckRecorded(tweak).Broken);
    }

    [Fact]
    public void The_step_survives_the_history_file_and_belongs_to_the_account_that_made_it()
    {
        var mine = new SetStartupEnabled(StartupOrigin.UserRun, "App", false);
        var ours = new SetStartupEnabled(StartupOrigin.MachineRun, "App", false);

        Assert.Equal(mine, System.Text.Json.JsonSerializer.Deserialize<Operation>(System.Text.Json.JsonSerializer.Serialize<Operation>(mine)));
        Assert.True(ChangeJournal.IsPerUser(mine));
        Assert.False(ChangeJournal.IsPerUser(ours));
        Assert.Equal("Stop App starting with Windows", mine.Describe());
        Assert.Equal("Start App with Windows", (mine with { Enabled = true }).Describe());
    }

    [Fact]
    public void An_entry_for_every_account_is_changed_in_the_machine_hive_and_one_for_you_in_yours()
    {
        Assert.Equal(TweakScope.Machine, StartupTweaks.TurnOff(Entry(StartupOrigin.MachineRun32, "A")).Scope);
        Assert.Equal(TweakScope.Machine, StartupTweaks.TurnOff(Entry(StartupOrigin.MachineFolder, "A.lnk")).Scope);
        Assert.Equal(TweakScope.User, StartupTweaks.TurnOff(Entry(StartupOrigin.UserFolder, "A.lnk")).Scope);
        Assert.Equal(TweakScope.User, StartupTweaks.TurnOff(Entry(StartupOrigin.UserRun, "A")).Scope);

        // The mark is written in the hive the entry belongs to.
        var user = Entry(StartupOrigin.UserRun, "Mine");
        var machine = Entry(StartupOrigin.MachineRun, "Ours");
        Assert.True(_engine.Apply(StartupTweaks.TurnOff(user), Guid.NewGuid()).Success);
        Assert.True(_engine.Apply(StartupTweaks.TurnOff(machine), Guid.NewGuid()).Success);
        Assert.True(_reg.Read(new RegistryTarget(RegistryHive.CurrentUser, StartupApproval.KeyOf(user.Origin), "Mine")).Exists);
        Assert.True(_reg.Read(new RegistryTarget(RegistryHive.LocalMachine, StartupApproval.KeyOf(machine.Origin), "Ours")).Exists);
    }
}
