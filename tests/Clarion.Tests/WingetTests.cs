using Clarion.Core.Apps;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class FakeWingetStore : IWingetStore
{
    public bool IsAvailable { get; set; } = true;
    public bool AcceptPackageAgreements { get; set; }
    public HashSet<string> Installed { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Calls { get; } = [];
    public int InstallCode { get; set; }
    public int UninstallCode { get; set; }
    public int Reads { get; private set; }
    public WingetPackageInfo? Info { get; set; }

    private IReadOnlySet<string>? _cache;

    // Like the real store, the installed list is read once and kept until it is invalidated.
    public IReadOnlySet<string> InstalledIds()
    {
        if (_cache is null) { Reads++; _cache = new HashSet<string>(Installed, StringComparer.OrdinalIgnoreCase); }
        return _cache;
    }

    public void Invalidate() => _cache = null;

    public WingetOutcome Install(string id, string? location = null)
    {
        Calls.Add(location is null ? "install " + id : $"install {id} at {location}");
        if (WingetCodes.IsSuccess(InstallCode)) Installed.Add(id);
        return new WingetOutcome(InstallCode, "");
    }

    public WingetOutcome Uninstall(string id)
    {
        Calls.Add("uninstall " + id);
        if (UninstallCode == 0) Installed.Remove(id);
        return new WingetOutcome(UninstallCode, "");
    }

    public WingetPackageInfo? Show(string id) => Info;
}

public sealed class WingetTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-winget-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch (IOException) { }
    }

    private const string SevenZip = """
        Found 7-Zip [7zip.7zip]
        Version: 26.04
        Publisher: Igor Pavlov
        Publisher Url: https://7-zip.org/
        Moniker: 7zip
        Homepage: https://7-zip.org/download.html
        License: LGPL-2.1
        License Url: https://7-zip.org/license.txt
        Installer:
          Installer Type: wix
          Installer Url: https://www.7-zip.org/a/7z2604-x64.msi
          Installer SHA256: 0b01334a418654293513449f61d6bfc99e5196ca371e3c3dc961eda57cd535c6
        """;

    // The same record with the labels in Spanish. Only the checksum and the address can be found without the English labels.
    private const string Spanish = """
        Encontrado 7-Zip [7zip.7zip]
        Versión: 26.04
        Editor: Igor Pavlov
        Descripción:
          Un programa para archivos comprimidos.
        Etiquetas:
          archive
          zip
        Instalador:
          Tipo de instalador: wix
          Dirección URL del instalador: https://www.7-zip.org/a/7z2604-x64.msi
          SHA256 del instalador: 0B01334A418654293513449F61D6BFC99E5196CA371E3C3DC961EDA57CD535C6
        """;

    [Theory]
    [InlineData("7zip.7zip")]
    [InlineData("Notepad++.Notepad++")]
    [InlineData("Rustlang.Rust.MSVC")]
    [InlineData("astral-sh.uv")]
    [InlineData("eloston.ungoogled-chromium")]
    public void Real_package_ids_are_accepted(string id) => Assert.True(WingetIds.IsValid(id));

    [Theory]
    [InlineData("")]
    [InlineData("nodots")]
    [InlineData("7zip.7zip --force")]
    [InlineData("7zip.7zip\" --override \"calc")]
    [InlineData("msstore:9NKSQGP7F2NH")]
    [InlineData("a.b\n--force")]
    [InlineData(".leading")]
    [InlineData("trailing.")]
    public void Anything_that_could_add_arguments_is_refused(string id) => Assert.False(WingetIds.IsValid(id));

    [Fact]
    public void An_install_is_pinned_to_one_exact_package_in_the_winget_source()
    {
        var command = WingetCommands.Install("7zip.7zip");

        Assert.StartsWith("install --id 7zip.7zip --exact --source winget", command);
        Assert.Contains("--silent", command);
        Assert.Contains("--accept-package-agreements", command);
        Assert.Contains("--disable-interactivity", command);
    }

    [Fact]
    public void Reads_and_removals_never_accept_package_licences()
    {
        Assert.DoesNotContain("--accept-package-agreements", WingetCommands.Uninstall("7zip.7zip"));
        Assert.DoesNotContain("--accept-package-agreements", WingetCommands.Show("7zip.7zip"));
        Assert.DoesNotContain("--accept-package-agreements", WingetCommands.Export(@"C:\x\y.json"));
        Assert.All(new[] { WingetCommands.Uninstall("a.b"), WingetCommands.Show("a.b") }, c => Assert.Contains("--exact", c));
    }

    [Fact]
    public void A_bad_id_never_becomes_a_command_line()
    {
        Assert.Throws<ArgumentException>(() => WingetCommands.Install("7zip.7zip --force"));
        Assert.Throws<ArgumentException>(() => WingetCommands.Uninstall("x"));
        Assert.Throws<ArgumentException>(() => WingetCommands.Export("C:\\a\"b.json"));
    }

    [Fact]
    public void The_show_text_is_read_into_its_fields()
    {
        var info = WingetShow.Parse(SevenZip)!;

        Assert.Equal("7zip.7zip", info.Id);
        Assert.Equal("7-Zip", info.Name);
        Assert.Equal("26.04", info.Version);
        Assert.Equal("Igor Pavlov", info.Publisher);
        Assert.Equal("LGPL-2.1", info.License);
        Assert.Equal("https://7-zip.org/license.txt", info.LicenseUrl);
        Assert.Equal("wix", info.InstallerType);
        Assert.Equal("www.7-zip.org", info.InstallerHost);
        Assert.Equal("0b01334a418654293513449f61d6bfc99e5196ca371e3c3dc961eda57cd535c6", info.Sha256);
    }

    [Fact]
    public void The_checksum_and_address_are_found_even_when_the_labels_are_in_another_language()
    {
        var info = WingetShow.Parse(Spanish)!;

        Assert.Equal("7zip.7zip", info.Id);
        Assert.Equal("7-Zip", info.Name);
        Assert.Equal("https://www.7-zip.org/a/7z2604-x64.msi", info.InstallerUrl);
        Assert.Equal("0b01334a418654293513449f61d6bfc99e5196ca371e3c3dc961eda57cd535c6", info.Sha256);
        Assert.Equal("", info.License);
    }

    [Theory]
    [InlineData("")]
    [InlineData("No package found matching input criteria.")]
    [InlineData("Found Something [A.B]\nVersion: 1\n")]
    public void Output_that_is_not_a_package_record_gives_nothing(string text) => Assert.Null(WingetShow.Parse(text));

    [Fact]
    public void The_export_file_gives_the_installed_ids_whatever_their_case()
    {
        const string json = """
            {"$schema":"x","CreationDate":"2026-10-11","WinGetVersion":"1.29","Sources":[
              {"SourceDetails":{"Name":"winget"},"Packages":[{"PackageIdentifier":"7zip.7zip"},{"PackageIdentifier":"Git.Git"}]},
              {"SourceDetails":{"Name":"other"},"Packages":[{"PackageIdentifier":"LocalSend.LocalSend"}]}]}
            """;

        var ids = WingetExport.InstalledIds(json);

        Assert.Equal(3, ids.Count);
        Assert.Contains("7ZIP.7ZIP", ids);
        Assert.Empty(WingetExport.InstalledIds("""{"Sources":[]}"""));
        Assert.Empty(WingetExport.InstalledIds("{}"));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(unchecked((int)0x8A150061), true)]
    [InlineData(unchecked((int)0x8A15010D), true)]
    [InlineData(unchecked((int)0x8A150109), true)]
    [InlineData(unchecked((int)0x8A150011), false)]
    [InlineData(unchecked((int)0x8A150010), false)]
    [InlineData(unchecked((int)0x8A15003A), false)]
    [InlineData(-1, false)]
    public void Documented_return_codes_say_whether_the_package_is_installed_now(int code, bool installed) =>
        Assert.Equal(installed, WingetCodes.IsSuccess(code));

    [Theory]
    [InlineData(unchecked((int)0x8A150011), "checksum")]
    [InlineData(unchecked((int)0x8A15003A), "Group Policy")]
    [InlineData(unchecked((int)0x8A150010), "fits this PC")]
    [InlineData(unchecked((int)0x8A150105), "space")]
    [InlineData(unchecked((int)0x8A150041), "licence")]
    [InlineData(-1, "too long")]
    public void Codes_become_sentences_a_person_can_act_on(int code, string expected) =>
        Assert.Contains(expected, WingetCodes.Explain(code), StringComparison.OrdinalIgnoreCase);

    [Fact]
    public void An_unknown_code_is_shown_in_hex_and_points_at_the_log() =>
        Assert.Contains("0x8A15FFFF", WingetCodes.Explain(unchecked((int)0x8A15FFFF)));

    [Fact]
    public void Restart_codes_count_as_installed_and_ask_for_a_restart()
    {
        Assert.True(WingetCodes.NeedsRestart(unchecked((int)0x8A150109)));
        Assert.False(WingetCodes.NeedsRestart(0));
    }

    // ---- the handler and the engine

    private static Tweak AppTweak(string id = "app.7zip", string package = "7zip.7zip") => new()
    {
        Id = id, Category = "Install", Topic = "Files", Name = "7-Zip", Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Proven, RiskLevel = RiskLevel.Safe, Scope = TweakScope.Machine,
        Apply = [new InstallWingetPackage(package)],
        Undo = [new UninstallWingetPackage(package)],
    };

    private TweakEngine Engine(FakeWingetStore store) =>
        new([new WingetHandler(store)], new ChangeJournal(Path.Combine(_dir, "journal.jsonl")));

    [Fact]
    public void Without_winget_an_app_is_unavailable_not_switched_off()
    {
        var store = new FakeWingetStore { IsAvailable = false };

        Assert.Equal(TweakState.Unavailable, Engine(store).Detect(AppTweak()));
    }

    [Fact]
    public void An_installed_package_reads_as_applied_and_a_missing_one_as_not_applied()
    {
        var store = new FakeWingetStore();
        var engine = Engine(store);

        Assert.Equal(TweakState.NotApplied, engine.Detect(AppTweak()));
        store.Installed.Add("7zip.7zip");
        engine.Invalidate();
        Assert.Equal(TweakState.Applied, engine.Detect(AppTweak()));
    }

    [Fact]
    public void Installing_needs_the_licence_to_have_been_accepted()
    {
        var store = new FakeWingetStore { AcceptPackageAgreements = false };

        var result = Engine(store).Apply(AppTweak(), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("licence", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(store.Calls);
    }

    [Fact]
    public void An_accepted_install_runs_and_is_journaled_and_revert_uninstalls_exactly_that_package()
    {
        var store = new FakeWingetStore { AcceptPackageAgreements = true };
        var engine = Engine(store);
        var tweak = AppTweak();

        Assert.True(engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Contains("7zip.7zip", store.Installed);
        Assert.Equal(["install 7zip.7zip"], store.Calls);

        Assert.True(engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.DoesNotContain("7zip.7zip", store.Installed);
        Assert.Equal(["install 7zip.7zip", "uninstall 7zip.7zip"], store.Calls);
    }

    [Fact]
    public void A_failed_install_reports_the_reason_and_leaves_nothing_recorded_as_on()
    {
        var store = new FakeWingetStore { AcceptPackageAgreements = true, InstallCode = unchecked((int)0x8A150011) };
        var engine = Engine(store);

        var result = engine.Apply(AppTweak(), Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Contains("checksum", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(TweakState.NotApplied, engine.Detect(AppTweak()));
    }

    [Fact]
    public void Uninstalling_something_that_is_already_gone_is_not_an_error()
    {
        var store = new FakeWingetStore { UninstallCode = WingetCodes.NoPackagesFound };
        var handler = new WingetHandler(store);

        handler.Execute(new UninstallWingetPackage("7zip.7zip"));

        Assert.Equal(["uninstall 7zip.7zip"], store.Calls);
    }

    [Fact]
    public void An_app_removed_by_hand_is_reported_by_name_when_verified()
    {
        var store = new FakeWingetStore { AcceptPackageAgreements = true };
        var engine = Engine(store);
        engine.Apply(AppTweak(), Guid.NewGuid());
        store.Installed.Clear();
        engine.Invalidate();

        var check = engine.CheckRecorded(AppTweak());

        Assert.Single(check.Broken);
        Assert.Contains("7zip.7zip is no longer installed", check.Broken[0].Reason);
    }

    [Fact]
    public void The_installed_list_is_read_once_for_many_apps()
    {
        var store = new FakeWingetStore();
        var engine = Engine(store);
        var tweaks = Enumerable.Range(0, 20).Select(i => AppTweak($"app.p{i}", $"Pub.Package{i}")).ToList();

        engine.WarmUp(tweaks);
        foreach (var t in tweaks) engine.Detect(t);

        Assert.Equal(1, store.Reads);
    }
}

public sealed class WingetCommandLineTests
{
    [Fact]
    public void The_licence_flag_is_off_unless_the_person_types_it()
    {
        Assert.False(Clarion.Core.Profiles.CliOptions.Parse(["--apply", "setup.json"]).AcceptPackageAgreements);
        Assert.True(Clarion.Core.Profiles.CliOptions.Parse(["--apply", "setup.json", "--accept-package-agreements"]).AcceptPackageAgreements);
    }

    [Fact]
    public void The_help_explains_that_the_flag_accepts_the_licence_terms()
    {
        Assert.Contains("--accept-package-agreements", Clarion.Core.Profiles.CliOptions.HelpText);
        Assert.Contains("licence terms", Clarion.Core.Profiles.CliOptions.HelpText);
    }
}
