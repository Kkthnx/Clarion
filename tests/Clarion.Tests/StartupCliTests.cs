using Clarion.Core.Profiles;
using Clarion.Core.Reports;
using Clarion.Core.Startup;

namespace Clarion.Tests;

public sealed class StartupCliTests
{
    private static StartupEntry Entry(string name, bool enabled, StartupOrigin origin = StartupOrigin.UserRun, bool found = true) =>
        new(origin, name, "cmd", enabled, null, new StartupProgram("C:\\x\\x.exe", found, "", ""), null);

    [Fact]
    public void The_command_is_parsed_and_listed_in_the_help()
    {
        Assert.Equal(CliMode.ListStartup, CliOptions.Parse(["--list-startup"]).Mode);
        Assert.Contains("--list-startup", CliOptions.HelpText, StringComparison.Ordinal);
    }

    [Fact]
    public void Each_entry_shows_whether_it_is_on_and_where_it_comes_from()
    {
        var text = CliReports.StartupEntries([Entry("Spotify", true), Entry("Corsair", false, StartupOrigin.MachineRun)]);

        Assert.Contains("on   Spotify", text, StringComparison.Ordinal);
        Assert.Contains("Your account", text, StringComparison.Ordinal);
        Assert.Contains("off  Corsair", text, StringComparison.Ordinal);
        Assert.Contains("Every account", text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_program_is_marked_and_an_empty_list_says_so()
    {
        Assert.Contains("(program not found)", CliReports.StartupEntries([Entry("Gone", true, found: false)]), StringComparison.Ordinal);
        Assert.Contains("Nothing is set to start", CliReports.StartupEntries([]), StringComparison.Ordinal);
    }

    [Fact]
    public void The_history_names_a_startup_change_in_plain_words()
    {
        var entry = new Clarion.Core.Journal.JournalEntry(Guid.NewGuid(), StartupTweaks.IdOf(StartupOrigin.UserRun, "Spotify"), Clarion.Core.Journal.JournalAction.Apply,
            DateTimeOffset.Now, new Clarion.Core.Model.SetStartupEnabled(StartupOrigin.UserRun, "Spotify", false), new Clarion.Core.Model.SetStartupEnabled(StartupOrigin.UserRun, "Spotify", true));
        var outstanding = new Dictionary<string, IReadOnlyList<Clarion.Core.Journal.JournalEntry>> { [entry.TweakId] = [entry] };

        var text = CliReports.History(outstanding, []);

        Assert.Contains("Stop Spotify starting with Windows", text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_ids_in_the_history_turn_into_changes_to_check()
    {
        var tweaks = StartupTweaks.ForIds([StartupTweaks.IdOf(StartupOrigin.MachineRun32, "Lightshot"), "privacy.telemetry-tasks"]);

        var tweak = Assert.Single(tweaks);
        Assert.Equal("startup.machine-run32.Lightshot", tweak.Id);
    }
}
