using System.Net;
using Clarion.Core.Updates;
using Clarion.Engine;

namespace Clarion.Tests;

public sealed class UpdateCheckTests
{
    private static ReleaseVersion V(string text) => ReleaseVersion.Parse(text)!;

    // ---- version order ----

    [Theory]
    [InlineData("0.1.0-beta.4", "0.1.0-beta.5")]
    [InlineData("0.1.0-beta.9", "0.1.0-beta.10")]
    [InlineData("0.1.0-beta.9", "0.1.0")]
    [InlineData("0.1.0", "0.1.1")]
    [InlineData("0.9.9", "1.0.0")]
    [InlineData("1.0.0-alpha", "1.0.0-beta")]
    [InlineData("1.0.0-beta", "1.0.0-beta.2")]
    [InlineData("1.0.0-1", "1.0.0-alpha")]
    public void Older_versions_sort_before_newer_ones(string older, string newer)
    {
        Assert.True(V(older) < V(newer), $"{older} should be older than {newer}");
        Assert.True(V(newer) > V(older));
    }

    [Theory]
    [InlineData("v0.1.0-beta.4", "0.1.0-beta.4")]
    [InlineData("0.1.0-beta.4+abc123", "0.1.0-beta.4")]
    public void A_leading_v_and_build_details_do_not_change_the_version(string a, string b) =>
        Assert.Equal(0, V(a).CompareTo(V(b)));

    [Theory]
    [InlineData("")]
    [InlineData("beta")]
    [InlineData("1.2")]
    [InlineData("1.2.x")]
    [InlineData("1.2.3-")]
    [InlineData("1.2.3-beta..1")]
    public void Text_that_is_not_a_version_is_refused(string text) => Assert.Null(ReleaseVersion.Parse(text));

    // ---- what to offer ----

    private static ReleaseInfo Rel(string tag, bool pre = true, bool draft = false, string? url = null) =>
        new(tag, "Clarion " + tag, url ?? $"https://github.com/Kkthnx/Clarion/releases/tag/{tag}", pre, draft, null);

    [Fact]
    public void Someone_on_a_beta_is_offered_a_newer_beta()
    {
        var offer = UpdateCheck.Newest("0.1.0-beta.4", [Rel("v0.1.0-beta.3"), Rel("v0.1.0-beta.4"), Rel("v0.1.0-beta.5")]);

        Assert.Equal("v0.1.0-beta.5", offer!.Tag);
        Assert.True(offer.PreRelease);
    }

    [Fact]
    public void The_newest_of_several_newer_releases_is_offered_and_beta_10_beats_beta_9()
    {
        var offer = UpdateCheck.Newest("0.1.0-beta.4", [Rel("v0.1.0-beta.9"), Rel("v0.1.0-beta.10"), Rel("v0.1.0-beta.5")]);
        Assert.Equal("v0.1.0-beta.10", offer!.Tag);
    }

    [Fact]
    public void A_final_release_is_offered_to_someone_on_a_beta_of_it()
    {
        var offer = UpdateCheck.Newest("0.1.0-beta.4", [Rel("v0.1.0-beta.5"), Rel("v0.1.0", pre: false)]);
        Assert.Equal("v0.1.0", offer!.Tag);
        Assert.False(offer.PreRelease);
    }

    [Fact]
    public void Someone_on_a_final_release_is_not_offered_a_beta()
    {
        Assert.Null(UpdateCheck.Newest("1.0.0", [Rel("v1.1.0-beta.1")]));
        Assert.Equal("v1.1.0", UpdateCheck.Newest("1.0.0", [Rel("v1.1.0-beta.1"), Rel("v1.1.0", pre: false)])!.Tag);
    }

    [Fact]
    public void Nothing_is_offered_when_the_newest_release_is_the_one_running_or_older()
    {
        Assert.Null(UpdateCheck.Newest("0.1.0-beta.4", [Rel("v0.1.0-beta.4"), Rel("v0.1.0-beta.3")]));
        Assert.Null(UpdateCheck.Newest("0.1.0-beta.4", []));
    }

    [Fact]
    public void Drafts_are_never_offered()
    {
        Assert.Null(UpdateCheck.Newest("0.1.0-beta.4", [Rel("v0.1.0-beta.5", draft: true)]));
    }

    [Fact]
    public void A_release_page_that_is_not_this_projects_is_never_offered()
    {
        Assert.Null(UpdateCheck.Newest("0.1.0-beta.4", [Rel("v0.1.0-beta.5", url: "https://evil.example/Kkthnx/Clarion/releases/tag/v0.1.0-beta.5")]));
        Assert.Null(UpdateCheck.Newest("0.1.0-beta.4", [Rel("v0.1.0-beta.5", url: "http://github.com/Kkthnx/Clarion/releases/tag/v0.1.0-beta.5")]));
        Assert.Null(UpdateCheck.Newest("0.1.0-beta.4", [Rel("v0.1.0-beta.5", url: "https://github.com/someone-else/Clarion/releases/tag/v0.1.0-beta.5")]));
    }

    [Fact]
    public void A_running_version_that_cannot_be_read_gets_no_offer_instead_of_a_wrong_one()
    {
        Assert.Null(UpdateCheck.Newest("dev", [Rel("v9.9.9", pre: false)]));
    }

    [Fact]
    public void A_version_the_person_hid_stays_hidden_until_a_newer_one_arrives()
    {
        var offer = new UpdateOffer("v0.1.0-beta.5", "n", "https://github.com/Kkthnx/Clarion/releases/tag/v0.1.0-beta.5", true);

        Assert.True(UpdateCheck.ShouldShow(offer, null));
        Assert.False(UpdateCheck.ShouldShow(offer, "v0.1.0-beta.5"));
        Assert.False(UpdateCheck.ShouldShow(offer, "v0.1.0-beta.6"));
        Assert.True(UpdateCheck.ShouldShow(offer with { Tag = "v0.1.0-beta.7" }, "v0.1.0-beta.5"));
    }

    // ---- reading the answer ----

    private const string Sample = """
        [
          { "tag_name": "v0.1.0-beta.5", "name": "Clarion 0.1.0 Beta 5", "html_url": "https://github.com/Kkthnx/Clarion/releases/tag/v0.1.0-beta.5",
            "draft": false, "prerelease": true, "published_at": "2026-11-01T10:00:00Z", "assets": [ { "name": "x" } ], "author": { "login": "a" } },
          { "tag_name": "v0.1.0-beta.4", "name": "Clarion 0.1.0 Beta 4", "html_url": "https://github.com/Kkthnx/Clarion/releases/tag/v0.1.0-beta.4",
            "draft": false, "prerelease": true, "published_at": "2026-10-09T19:06:35Z" }
        ]
        """;

    [Fact]
    public void A_release_list_in_the_shape_github_sends_is_read()
    {
        var list = UpdateCheck.ParseReleases(Sample);

        Assert.Equal(2, list.Count);
        Assert.Equal("v0.1.0-beta.5", list[0].Tag);
        Assert.True(list[0].PreRelease);
        Assert.False(list[0].Draft);
        Assert.Equal(new DateTimeOffset(2026, 11, 1, 10, 0, 0, TimeSpan.Zero), list[0].Published);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{\"message\":\"Not Found\"}")]
    [InlineData("[1, 2, \"x\", null, {}]")]
    public void An_answer_that_is_not_a_release_list_gives_no_releases_and_no_error(string json) =>
        Assert.Empty(UpdateCheck.ParseReleases(json));

    private sealed class FakeSource(string json) : IReleaseSource
    {
        public Task<string> GetReleasesJsonAsync(CancellationToken cancel) => Task.FromResult(json);
    }

    [Fact]
    public async Task The_whole_check_finds_the_newer_release()
    {
        var offer = await UpdateCheck.RunAsync(new FakeSource(Sample), "0.1.0-beta.4");
        Assert.Equal("v0.1.0-beta.5", offer!.Tag);
    }

    // ---- the network side ----

    private sealed class Canned(HttpStatusCode code, string body = "[]") : HttpMessageHandler
    {
        public HttpRequestMessage? Seen { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Seen = request;
            return Task.FromResult(new HttpResponseMessage(code) { Content = new StringContent(body) });
        }
    }

    [Fact]
    public async Task The_request_goes_only_to_the_releases_list_and_identifies_itself()
    {
        var handler = new Canned(HttpStatusCode.OK, Sample);

        var json = await new GitHubReleaseSource("0.1.0-beta.4", handler).GetReleasesJsonAsync(default);

        Assert.Equal(Sample, json);
        Assert.Equal("api.github.com", handler.Seen!.RequestUri!.Host);
        Assert.StartsWith("/repos/Kkthnx/Clarion/releases", handler.Seen.RequestUri.AbsolutePath);
        Assert.Contains("Clarion/0.1.0-beta.4", handler.Seen.Headers.UserAgent.ToString());
        Assert.Null(handler.Seen.Headers.Authorization);
        Assert.False(handler.Seen.Headers.Contains("Cookie"));
    }

    [Theory]
    [InlineData(HttpStatusCode.Forbidden, "limiting")]
    [InlineData(HttpStatusCode.TooManyRequests, "limiting")]
    [InlineData(HttpStatusCode.InternalServerError, "error")]
    public async Task A_refusal_or_error_becomes_a_plain_message(HttpStatusCode code, string expected)
    {
        var ex = await Assert.ThrowsAsync<UpdateCheckException>(() => new GitHubReleaseSource("1", new Canned(code)).GetReleasesJsonAsync(default));
        Assert.Contains(expected, ex.Message);
    }

    private sealed class Unreachable : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            throw new HttpRequestException("no network");
    }

    [Fact]
    public async Task No_network_becomes_a_plain_message()
    {
        var ex = await Assert.ThrowsAsync<UpdateCheckException>(() => new GitHubReleaseSource("1", new Unreachable()).GetReleasesJsonAsync(default));
        Assert.Contains("internet connection", ex.Message);
    }
}
