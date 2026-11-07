using Clarion.Core.Diagnostics;

namespace Clarion.Tests;

public sealed class SessionMarkerTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-marker-" + Guid.NewGuid().ToString("N"));
    private string File1 => Path.Combine(_dir, "session.txt");
    private static readonly DateTime Boot = new(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Now = new(2026, 11, 2, 9, 0, 0, DateTimeKind.Utc);

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }

    private SessionMarker Make(bool otherRunning = false, DateTime? boot = null) =>
        new(File1, _ => otherRunning, () => boot ?? Boot);

    [Fact]
    public void A_first_start_has_nothing_to_report_and_leaves_a_marker()
    {
        var marker = Make();

        Assert.Null(marker.Begin(100, "1.5.0", Now));
        Assert.True(File.Exists(File1));
    }

    [Fact]
    public void Closing_normally_removes_the_marker_so_the_next_start_reports_nothing()
    {
        var first = Make();
        first.Begin(100, "1.5.0", Now);
        first.End();

        Assert.False(File.Exists(File1));
        Assert.Null(Make().Begin(101, "1.5.0", Now.AddHours(1)));
    }

    [Fact]
    public void A_session_that_never_closed_is_reported_with_its_version_and_the_page_it_was_on()
    {
        var first = Make();
        first.Begin(100, "1.4.0", Now);
        first.Page("Clean up");

        var ended = Make().Begin(101, "1.5.0", Now.AddHours(1));

        Assert.NotNull(ended);
        Assert.Equal("1.4.0", ended!.Version);
        Assert.Equal("Clean up", ended.LastPage);
        Assert.Equal(Now, ended.StartedUtc);
    }

    [Fact]
    public void The_report_is_given_once_because_the_new_session_takes_the_marker_over()
    {
        var first = Make();
        first.Begin(100, "1.4.0", Now);
        var second = Make();
        Assert.NotNull(second.Begin(101, "1.5.0", Now.AddHours(1)));
        second.End();

        Assert.Null(Make().Begin(102, "1.5.0", Now.AddHours(2)));
    }

    [Fact]
    public void A_restart_of_windows_after_the_session_began_is_not_a_crash()
    {
        Make().Begin(100, "1.4.0", Now);

        Assert.Null(Make(boot: Now.AddMinutes(30)).Begin(101, "1.5.0", Now.AddHours(1)));
    }

    [Fact]
    public void A_copy_that_is_still_running_keeps_its_marker()
    {
        var running = Make();
        running.Begin(100, "1.4.0", Now);
        running.Page("Home");

        var other = Make(otherRunning: true);
        Assert.Null(other.Begin(101, "1.5.0", Now.AddHours(1)));
        // The second copy does not own the marker, so closing it must not remove the first one's.
        other.End();
        Assert.True(File.Exists(File1));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a marker")]
    [InlineData("abc\n1.4.0\nnot a date\nHome")]
    public void A_damaged_marker_is_ignored_and_replaced(string content)
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(File1, content);

        Assert.Null(Make().Begin(100, "1.5.0", Now));
        Assert.StartsWith("100\n", File.ReadAllText(File1));
    }

    [Fact]
    public void A_page_name_cannot_break_the_file_into_more_lines()
    {
        var marker = Make();
        marker.Begin(100, "1.4.0", Now);
        marker.Page("Clean\nup\r");

        var ended = Make().Begin(101, "1.5.0", Now.AddHours(1));
        Assert.Equal("Clean up ", ended!.LastPage);
    }

    [Fact]
    public void Writing_into_a_folder_that_cannot_be_written_does_not_throw()
    {
        var blocked = new SessionMarker(Path.Combine(_dir, "a\0b", "session.txt"), _ => false, () => Boot);

        var ex = Record.Exception(() => { blocked.Begin(100, "1.5.0", Now); blocked.Page("Home"); blocked.End(); });
        Assert.Null(ex);
    }
}
