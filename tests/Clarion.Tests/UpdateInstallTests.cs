using System.Net;
using System.Security.Cryptography;
using Clarion.Core.Updates;
using Clarion.Engine;

namespace Clarion.Tests;

public sealed class UpdateInstallTests : IDisposable
{
    private const string Tag = "v1.8.0";
    private const string SetupUrl = "https://github.com/Kkthnx/Clarion/releases/download/v1.8.0/Clarion-1.8.0-setup.exe";
    private const string ChecksumUrl = "https://github.com/Kkthnx/Clarion/releases/download/v1.8.0/Clarion-1.8.0-setup.exe.sha256";
    private static readonly byte[] Payload = [.. Enumerable.Range(0, 300_000).Select(i => (byte)(i * 7))];
    private static readonly string PayloadHash = Convert.ToHexString(SHA256.HashData(Payload)).ToLowerInvariant();

    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-upd-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static ReleaseInfo Release(params ReleaseAsset[] assets) =>
        new(Tag, ".Clarion 1.8.0", "https://github.com/Kkthnx/Clarion/releases/tag/v1.8.0", false, false, null, "", assets);

    private static ReleaseAsset Setup(string? sha = null, long? size = null, string url = SetupUrl) =>
        new("Clarion-1.8.0-setup.exe", url, size ?? Payload.Length, sha);

    [Fact]
    public void Release_files_are_read_with_their_digest()
    {
        var json = $$"""
            [{"tag_name":"v1.8.0","name":".Clarion 1.8.0","html_url":"https://github.com/Kkthnx/Clarion/releases/tag/v1.8.0","prerelease":false,"draft":false,
              "assets":[{"name":"Clarion-1.8.0-setup.exe","browser_download_url":"{{SetupUrl}}","size":62822373,"digest":"sha256:{{PayloadHash.ToUpperInvariant()}}"},
                        {"name":"notes.txt","browser_download_url":"https://github.com/Kkthnx/Clarion/releases/download/v1.8.0/notes.txt","size":10,"digest":"md5:abc"}]}]
            """;
        var release = Assert.Single(UpdateCheck.ParseReleases(json));
        Assert.Equal(2, release.Assets!.Count);
        Assert.Equal(PayloadHash, release.Assets[0].Sha256);
        Assert.Null(release.Assets[1].Sha256);
        Assert.Equal(62822373, UpdateInstall.SetupOf(release)!.Size);
        // The offer carries the release, so the files go with it to the download.
        Assert.Same(release, UpdateCheck.Newest("1.7.0", [release])!.Release);
    }

    [Theory]
    [InlineData("https://github.com/Kkthnx/Clarion/releases/download/v1.8.0/Clarion-1.8.0-setup.exe", true)]
    [InlineData("http://github.com/Kkthnx/Clarion/releases/download/v1.8.0/Clarion-1.8.0-setup.exe", false)]
    [InlineData("https://github.com/someone/Clarion/releases/download/v1.8.0/Clarion-1.8.0-setup.exe", false)]
    [InlineData("https://github.com/Kkthnx/Clarion/releases/download/v1.7.0/Clarion-1.8.0-setup.exe", false)]
    [InlineData("https://github.com.example.net/Kkthnx/Clarion/releases/download/v1.8.0/Clarion-1.8.0-setup.exe", false)]
    public void Only_this_projects_download_address_for_the_offered_tag_is_trusted(string url, bool trusted) =>
        Assert.Equal(trusted, UpdateInstall.IsTrustedDownloadUrl(url, Tag));

    [Fact]
    public void The_setup_program_must_have_its_expected_name_a_trusted_address_and_a_sane_size()
    {
        Assert.NotNull(UpdateInstall.SetupOf(Release(Setup())));
        Assert.Null(UpdateInstall.SetupOf(Release(Setup() with { Name = "Clarion-1.8.0-win-x64.zip" })));
        Assert.Null(UpdateInstall.SetupOf(Release(Setup(url: "https://example.com/Clarion-1.8.0-setup.exe"))));
        Assert.Null(UpdateInstall.SetupOf(Release(Setup(size: 0))));
        Assert.Null(UpdateInstall.SetupOf(Release(Setup(size: UpdateInstall.MaxSetupBytes + 1))));
        Assert.Null(UpdateInstall.SetupOf(Release()));
    }

    [Theory]
    [InlineData("sha256:ABCDEF0123456789abcdef0123456789abcdef0123456789abcdef0123456789", "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789")]
    [InlineData("sha256:abc", null)]
    [InlineData("sha512:abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789", null)]
    [InlineData(null, null)]
    public void A_digest_is_read_only_when_it_is_a_full_sha256(string? digest, string? expected) =>
        Assert.Equal(expected, UpdateInstall.ParseDigest(digest));

    [Fact]
    public void The_checksum_file_the_release_carries_is_read_for_the_right_file()
    {
        var hex = new string('a', 64);
        Assert.Equal(hex, UpdateInstall.ParseChecksumFile($"{hex}  Clarion-1.8.0-setup.exe\r\n", "Clarion-1.8.0-setup.exe"));
        Assert.Equal(hex, UpdateInstall.ParseChecksumFile($"{hex.ToUpperInvariant()} *Clarion-1.8.0-setup.exe", "Clarion-1.8.0-setup.exe"));
        Assert.Null(UpdateInstall.ParseChecksumFile($"{hex}  Clarion-1.8.0-win-x64.zip", "Clarion-1.8.0-setup.exe"));
        Assert.Null(UpdateInstall.ParseChecksumFile("not a hash  Clarion-1.8.0-setup.exe", "Clarion-1.8.0-setup.exe"));
    }

    [Theory]
    [InlineData(@"C:\Program Files\Clarion\", @"C:\Program Files\Clarion\", true)]
    [InlineData(@"C:\Program Files\Clarion\", @"c:\program files\clarion", true)]
    [InlineData(@"C:\Users\me\Downloads\Clarion-1.7.0-win-x64\", @"C:\Program Files\Clarion\", false)]
    [InlineData(@"C:\Program Files\Clarion\", null, false)]
    [InlineData(@"C:\Program Files\Clarion\", "", false)]
    public void Only_the_installed_copy_updates_itself(string running, string? installed, bool expected) =>
        Assert.Equal(expected, UpdateInstall.IsInstalledCopy(running, installed));

    [Fact]
    public void Setup_runs_silently_without_a_restart_and_starts_Clarion_again()
    {
        var args = UpdateInstall.SetupArguments(@"C:\ProgramData\Clarion\updates\setup.log");
        Assert.Contains("/SILENT", args);
        Assert.Contains("/NORESTART", args);
        Assert.Contains("/RELAUNCH", args);
        Assert.Contains("/LOG=\"C:\\ProgramData\\Clarion\\updates\\setup.log\"", args);
    }

    [Fact]
    public async Task A_download_that_matches_its_digest_is_kept_under_its_real_name()
    {
        var path = await Downloader(new Files(Payload)).DownloadSetupAsync(Release(Setup(PayloadHash)), null, default);
        Assert.Equal(Path.Combine(_dir, "Clarion-1.8.0-setup.exe"), path);
        Assert.Equal(PayloadHash, UpdateInstall.Sha256Of(path));
        Assert.Empty(Directory.EnumerateFiles(_dir, "*.part"));
    }

    [Fact]
    public async Task A_download_that_does_not_match_is_deleted_and_never_returned()
    {
        var wrong = new string('0', 64);
        var ex = await Assert.ThrowsAsync<UpdateCheckException>(() => Downloader(new Files(Payload)).DownloadSetupAsync(Release(Setup(wrong)), null, default));
        Assert.Contains("does not match", ex.Message);
        Assert.Empty(Directory.EnumerateFiles(_dir));
    }

    [Fact]
    public async Task A_download_larger_than_the_release_says_is_stopped()
    {
        var ex = await Assert.ThrowsAsync<UpdateCheckException>(() =>
            Downloader(new Files(Payload)).DownloadSetupAsync(Release(Setup(PayloadHash, size: 1000)), null, default));
        Assert.Contains("larger", ex.Message);
        Assert.Empty(Directory.EnumerateFiles(_dir));
    }

    [Fact]
    public async Task Without_a_digest_the_checksum_file_is_used_and_without_either_nothing_is_downloaded()
    {
        var checksum = new ReleaseAsset("Clarion-1.8.0-setup.exe.sha256", ChecksumUrl, 91, null);
        var files = new Files(Payload, $"{PayloadHash}  Clarion-1.8.0-setup.exe");
        var path = await Downloader(files).DownloadSetupAsync(Release(Setup(), checksum), null, default);
        Assert.Equal(PayloadHash, UpdateInstall.Sha256Of(path));

        var none = new Files(Payload);
        var ex = await Assert.ThrowsAsync<UpdateCheckException>(() => Downloader(none).DownloadSetupAsync(Release(Setup()), null, default));
        Assert.Contains("no checksum", ex.Message);
        Assert.Equal(0, none.SetupRequests);
    }

    [Fact]
    public async Task Progress_reaches_the_whole_file_and_an_older_setup_program_is_removed_first()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path.Combine(_dir, "Clarion-1.7.0-setup.exe"), "old");
        var seen = new List<double>();
        await Downloader(new Files(Payload)).DownloadSetupAsync(Release(Setup(PayloadHash)), new SyncProgress(seen.Add), default);
        Assert.Equal(1.0, seen[^1], 3);
        Assert.False(File.Exists(Path.Combine(_dir, "Clarion-1.7.0-setup.exe")));
    }

    [Fact]
    public async Task Stopping_the_download_leaves_no_file()
    {
        using var stop = new CancellationTokenSource();
        await stop.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Downloader(new Files(Payload)).DownloadSetupAsync(Release(Setup(PayloadHash)), null, stop.Token));
        Assert.True(!Directory.Exists(_dir) || !Directory.EnumerateFiles(_dir).Any());
    }

    private UpdateDownloader Downloader(HttpMessageHandler handler) => new("1.7.0", _dir, handler);

    private sealed class SyncProgress(Action<double> report) : IProgress<double>
    {
        public void Report(double value) => report(value);
    }

    /// <summary>Answers the two release addresses from memory, so no test touches the network.</summary>
    private sealed class Files(byte[] setup, string? checksum = null) : HttpMessageHandler
    {
        public int SetupRequests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancel)
        {
            cancel.ThrowIfCancellationRequested();
            var url = request.RequestUri!.AbsoluteUri;
            HttpResponseMessage response;
            if (url == SetupUrl) { SetupRequests++; response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(setup) }; }
            else if (url == ChecksumUrl && checksum is not null) response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(checksum) };
            else response = new HttpResponseMessage(HttpStatusCode.NotFound);
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}
