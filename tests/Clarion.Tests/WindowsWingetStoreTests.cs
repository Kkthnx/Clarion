using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Clarion.Core.Abstractions;
using Clarion.Core.Apps;
using Clarion.Engine;

namespace Clarion.Tests;

[SupportedOSPlatform("windows")]
public sealed class WindowsWingetStoreTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-winget-store-" + Guid.NewGuid().ToString("N"));
    private readonly string _exe;

    public WindowsWingetStoreTests()
    {
        Directory.CreateDirectory(_dir);
        _exe = Path.Combine(_dir, "winget.exe");
        File.WriteAllText(_exe, "");
    }

    public void Dispose()
    {
        try { Directory.Delete(_dir, true); } catch (IOException) { }
    }

    private sealed class Runner(Func<string, ProcessResult> respond) : IProcessRunner
    {
        public List<(string File, string Args)> Calls { get; } = [];

        public ProcessResult Run(string fileName, string arguments, TimeSpan timeout)
        {
            Calls.Add((fileName, arguments));
            return respond(arguments);
        }
    }

    private static ProcessResult WriteExport(string arguments, string json)
    {
        var path = Regex.Match(arguments, "--output \"([^\"]+)\"").Groups[1].Value;
        File.WriteAllText(path, json);
        return new ProcessResult(0, "", "");
    }

    private const string Export = """{"Sources":[{"Packages":[{"PackageIdentifier":"Git.Git"},{"PackageIdentifier":"7zip.7zip"}]}]}""";

    [Fact]
    public void It_is_available_only_when_winget_is_at_the_place_it_is_expected()
    {
        Assert.True(new WindowsWingetStore(new Runner(_ => new ProcessResult(0, "", "")), _exe).IsAvailable);
        Assert.False(new WindowsWingetStore(new Runner(_ => new ProcessResult(0, "", "")), Path.Combine(_dir, "missing.exe")).IsAvailable);
    }

    [Fact]
    public void The_default_place_is_the_account_s_own_app_alias_folder_and_not_a_path_search()
    {
        var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "winget.exe");

        Assert.Equal(expected, WindowsWingetStore.DefaultPath);
        Assert.True(Path.IsPathRooted(WindowsWingetStore.DefaultPath));
    }

    [Fact]
    public void The_installed_list_comes_from_one_export_and_is_kept_until_invalidated()
    {
        var runner = new Runner(a => WriteExport(a, Export));
        var store = new WindowsWingetStore(runner, _exe);

        var first = store.InstalledIds();
        var second = store.InstalledIds();

        Assert.Equal(2, first.Count);
        Assert.Contains("git.git", second);
        Assert.Single(runner.Calls);
        Assert.Equal(_exe, runner.Calls[0].File);
        Assert.StartsWith("export --output", runner.Calls[0].Args);

        store.Invalidate();
        store.InstalledIds();
        Assert.Equal(2, runner.Calls.Count);
    }

    [Fact]
    public void The_export_file_is_not_left_behind()
    {
        string? written = null;
        var store = new WindowsWingetStore(new Runner(a => { written = Regex.Match(a, "--output \"([^\"]+)\"").Groups[1].Value; return WriteExport(a, Export); }), _exe);

        store.InstalledIds();

        Assert.NotNull(written);
        Assert.False(File.Exists(written));
    }

    [Fact]
    public void A_warning_exit_code_with_a_written_file_still_counts()
    {
        var store = new WindowsWingetStore(new Runner(a => { WriteExport(a, Export); return new ProcessResult(unchecked((int)0x8A150035), "", ""); }), _exe);

        Assert.Equal(2, store.InstalledIds().Count);
    }

    [Fact]
    public void A_failed_read_is_remembered_so_each_app_does_not_run_winget_again()
    {
        var runner = new Runner(_ => new ProcessResult(unchecked((int)0x8A15004B), "", ""));
        var store = new WindowsWingetStore(runner, _exe);

        var first = Assert.Throws<InvalidOperationException>(() => store.InstalledIds());
        Assert.Throws<InvalidOperationException>(() => store.InstalledIds());

        Assert.Contains("source", first.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(runner.Calls);
    }

    [Fact]
    public void Install_and_uninstall_run_the_pinned_command_lines_and_pass_the_exit_code_on()
    {
        var runner = new Runner(a => new ProcessResult(a.StartsWith("install", StringComparison.Ordinal) ? 0 : unchecked((int)0x8A150030), "out", "err"));
        var store = new WindowsWingetStore(runner, _exe);

        var install = store.Install("7zip.7zip");
        var uninstall = store.Uninstall("7zip.7zip");

        Assert.True(install.Succeeded);
        Assert.False(uninstall.Succeeded);
        Assert.Contains("did not finish", uninstall.Message);
        Assert.Equal(WingetCommands.Install("7zip.7zip"), runner.Calls[0].Args);
        Assert.Equal(WingetCommands.Uninstall("7zip.7zip"), runner.Calls[1].Args);
        Assert.Contains("err", uninstall.Output);
    }

    [Fact]
    public void A_bad_package_id_stops_before_anything_is_run()
    {
        var runner = new Runner(_ => new ProcessResult(0, "", ""));
        var store = new WindowsWingetStore(runner, _exe);

        Assert.Throws<ArgumentException>(() => store.Install("7zip.7zip --force"));
        Assert.Empty(runner.Calls);
    }

    [Fact]
    public void Show_returns_the_details_or_nothing()
    {
        const string text = "Found 7-Zip [7zip.7zip]\nVersion: 26.04\nInstaller:\n  Installer Url: https://x.example/a.msi\n  Installer SHA256: 0b01334a418654293513449f61d6bfc99e5196ca371e3c3dc961eda57cd535c6\n";

        var found = new WindowsWingetStore(new Runner(_ => new ProcessResult(0, text, "")), _exe).Show("7zip.7zip");
        var missing = new WindowsWingetStore(new Runner(_ => new ProcessResult(unchecked((int)0x8A150014), "No package found.", "")), _exe).Show("7zip.7zip");

        Assert.Equal("x.example", found!.InstallerHost);
        Assert.Null(missing);
    }
}
