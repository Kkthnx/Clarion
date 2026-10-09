using Clarion.Core.Settings;

namespace Clarion.Tests;

public sealed class UserSettingsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-settings-" + Guid.NewGuid().ToString("N"));
    private string FilePath => Path.Combine(_dir, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    [Theory]
    [InlineData(false, 0, true)]
    [InlineData(false, 3, false)]
    [InlineData(true, 0, false)]
    [InlineData(true, 3, false)]
    public void The_welcome_is_only_for_someone_who_has_not_seen_it_and_has_nothing_applied(bool done, int applied, bool welcome) =>
        Assert.Equal(welcome, new UserSettings { FirstRunDone = done }.NeedsWelcome(applied));

    [Fact]
    public void With_no_file_the_defaults_are_used_and_nothing_risky_is_on()
    {
        var s = new UserSettingsStore(FilePath).Load();

        Assert.Equal("System", s.Theme);
        Assert.False(s.ExpertMode);
        Assert.False(s.FirstRunDone);
        Assert.False(s.CheckForUpdates);
        Assert.False(s.MonthlyVerify);
        Assert.Null(s.LastUpdateCheck);
    }

    [Fact]
    public void Choices_survive_a_restart()
    {
        var store = new UserSettingsStore(FilePath);
        var when = new DateTimeOffset(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

        Assert.True(store.Save(new UserSettings
        {
            Theme = "Dark", ExpertMode = true, FirstRunDone = true, CheckForUpdates = true,
            LastUpdateCheck = when, DismissedUpdate = "v0.1.0-beta.5", MonthlyVerify = true,
        }));

        var loaded = new UserSettingsStore(FilePath).Load();
        Assert.Equal("Dark", loaded.Theme);
        Assert.True(loaded.ExpertMode && loaded.FirstRunDone && loaded.CheckForUpdates && loaded.MonthlyVerify);
        Assert.Equal(when, loaded.LastUpdateCheck);
        Assert.Equal("v0.1.0-beta.5", loaded.DismissedUpdate);
    }

    [Fact]
    public void A_damaged_file_gives_the_defaults_instead_of_an_error()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "{ this is not json");

        Assert.Equal(new UserSettings(), new UserSettingsStore(FilePath).Load());
    }

    [Fact]
    public void An_unknown_theme_name_is_treated_as_the_system_theme()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "{\"Theme\":\"Purple\",\"ExpertMode\":true}");

        var s = new UserSettingsStore(FilePath).Load();

        Assert.Equal("System", s.Theme);
        Assert.True(s.ExpertMode);
    }

    [Fact]
    public void A_file_from_a_newer_version_with_extra_fields_still_loads()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(FilePath, "{\"Theme\":\"Light\",\"SomethingNew\":42}");

        Assert.Equal("Light", new UserSettingsStore(FilePath).Load().Theme);
    }

    [Fact]
    public void Saving_leaves_no_temporary_file_behind_and_replaces_the_old_one()
    {
        var store = new UserSettingsStore(FilePath);
        store.Save(new UserSettings { Theme = "Light" });
        store.Save(new UserSettings { Theme = "Dark" });

        Assert.False(File.Exists(FilePath + ".tmp"));
        Assert.Equal("Dark", store.Load().Theme);
    }

    [Fact]
    public void A_file_that_cannot_be_written_reports_false_and_does_not_throw()
    {
        // A folder where the file should be makes the write fail.
        Directory.CreateDirectory(FilePath);

        Assert.False(new UserSettingsStore(FilePath).Save(new UserSettings()));
    }
}
