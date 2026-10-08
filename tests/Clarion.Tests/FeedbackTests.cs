using Clarion.Core.Feedback;
using Clarion.Core.Model;
using Clarion.Core.SystemInfo;

namespace Clarion.Tests;

public sealed class FeedbackTests
{
    private const string User = "jsmith";
    private const string Machine = "DESKTOP-4F2K9Q";

    [Theory]
    [InlineData(@"Cannot open C:\Users\jsmith\AppData\Local\x.log", @"Cannot open C:\Users\<user>\AppData\Local\x.log")]
    [InlineData("Machine DESKTOP-4F2K9Q failed", "Machine <computer> failed")]
    [InlineData("mail me at jane.doe@example.com now", "mail me at <email> now")]
    [InlineData("server 192.168.1.20 down", "server <ip address> down")]
    [InlineData("mac 00:1A:2B:3C:4D:5E here", "mac <mac address> here")]
    [InlineData("sid S-1-5-21-1111111111-2222222222-3333333333-1001 owns it", "sid <account id> owns it")]
    [InlineData("addr 2001:0db8:85a3:0000:0000:8a2e:0370:7334 up", "addr <ip address> up")]
    public void Personal_details_are_removed(string input, string expected) =>
        Assert.Equal(expected, Sanitizer.Clean(input, User, Machine));

    [Theory]
    [InlineData("driver 32.0.16.1692 build 26200.9457")]
    [InlineData("Windows 11 Pro, 25H2")]
    [InlineData("Ryzen 7 7800X3D, 8 cores")]
    public void Version_numbers_and_hardware_names_are_left_alone(string input) =>
        Assert.Equal(input, Sanitizer.Clean(input, User, Machine));

    [Fact]
    public void Very_short_names_are_not_replaced_so_the_text_stays_readable() =>
        Assert.Equal("a b c", Sanitizer.Clean("a b c", "a", "b"));

    private static FeedbackInput Input(bool system = true, bool log = true, SystemReport? report = null, string desc = "It crashed") => new(
        FeedbackKind.Bug, "Crash on apply", desc, "0.1.0-beta.2", system, log, report,
        ["privacy.a", "input.b"], [@"2026-10-08 12:00:00 error at C:\Users\jsmith\x", "second line"]);

    private static SystemReport Report() => new(
        [new FactGroup("Windows", [new("Edition", "Windows 11 Pro"), new("Last restart", "today"), new("Installed", "3/22/2025")]),
         new FactGroup("Hardware", [new("Computer", "Example Board 1"), new("Processor", "Example CPU")])],
        new InstallVerdict(InstallLevel.Standard, []));

    [Fact]
    public void The_report_has_the_choices_and_leaves_out_what_identifies_the_pc()
    {
        var text = FeedbackReport.Build(Input(report: Report()), User, Machine);

        Assert.Contains("Windows 11 Pro", text);
        Assert.Contains("Example CPU", text);
        Assert.Contains("0.1.0-beta.2", text);
        Assert.Contains("privacy.a, input.b".Replace("privacy.a, input.b", "input.b, privacy.a"), text);
        Assert.DoesNotContain("Last restart", text);
        Assert.DoesNotContain("Installed:", text);
        Assert.DoesNotContain(User, text);
        Assert.Contains(@"C:\Users\<user>\x", text);
    }

    [Fact]
    public void System_details_and_log_are_left_out_when_not_chosen()
    {
        var text = FeedbackReport.Build(Input(system: false, log: false, report: Report()), User, Machine);

        Assert.DoesNotContain("Windows 11 Pro", text);
        Assert.DoesNotContain("Recent activity", text);
        Assert.Contains("It crashed", text);
    }

    [Fact]
    public void The_persons_own_words_are_cleaned_too()
    {
        var text = FeedbackReport.Build(Input(desc: "My PC DESKTOP-4F2K9Q at 10.0.0.5 broke"), User, Machine);
        Assert.Contains("My PC <computer> at <ip address> broke", text);
    }

    [Fact]
    public void Titles_get_a_prefix_and_a_fallback()
    {
        Assert.Equal("Bug: Crash on apply", FeedbackReport.CleanTitle(Input(), User, Machine));
        Assert.Equal("Bug: Problem report", FeedbackReport.CleanTitle(Input() with { Title = "  " }, User, Machine));
        Assert.StartsWith("Suggestion: ", FeedbackReport.CleanTitle(Input() with { Kind = FeedbackKind.Suggestion }, User, Machine));
    }

    [Fact]
    public void Short_reports_travel_in_the_address_and_long_ones_fall_back_to_paste()
    {
        var (url, needsPaste) = FeedbackReport.IssueUrl(FeedbackKind.Bug, "Bug: x", "short body & more");
        Assert.False(needsPaste);
        Assert.Contains("labels=bug", url.AbsoluteUri);
        Assert.Contains("short%20body%20%26%20more", url.AbsoluteUri);
        Assert.Equal("github.com", url.Host);

        var (longUrl, paste) = FeedbackReport.IssueUrl(FeedbackKind.Suggestion, "Suggestion: x", new string('a', 20000));
        Assert.True(paste);
        Assert.True(longUrl.AbsoluteUri.Length < FeedbackReport.MaxUrlLength);
        Assert.Contains("labels=enhancement", longUrl.AbsoluteUri);
    }
}
