using System.Net;
using System.Net.Http.Headers;
using Clarion.Core.Updates;

namespace Clarion.Engine;

/// <summary>
/// Asks GitHub for the list of this project's releases. That is the only address it ever contacts, it sends no cookies and no
/// account details, and it needs no sign-in. GitHub requires a User-Agent and allows 60 anonymous requests an hour. The list is used
/// instead of the "latest" address, which skips pre-releases and so finds nothing while every release is a beta.
/// </summary>
public sealed class GitHubReleaseSource(string appVersion, HttpMessageHandler? handler = null) : IReleaseSource
{
    private static readonly Uri Address = new($"https://api.github.com/repos/{UpdateCheck.Repository}/releases?per_page=15");

    public async Task<string> GetReleasesJsonAsync(CancellationToken cancel)
    {
        using var client = new HttpClient(handler ?? new HttpClientHandler { UseCookies = false }) { Timeout = TimeSpan.FromSeconds(12) };
        using var request = new HttpRequestMessage(HttpMethod.Get, Address);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Clarion", appVersion));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        try
        {
            using var response = await client.SendAsync(request, cancel);
            if (response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.TooManyRequests)
                throw new UpdateCheckException("GitHub is limiting requests from this network right now. Try again in an hour.");
            if (!response.IsSuccessStatusCode)
                throw new UpdateCheckException($"GitHub answered with an error ({(int)response.StatusCode}). Try again later.");
            return await response.Content.ReadAsStringAsync(cancel);
        }
        catch (HttpRequestException)
        {
            throw new UpdateCheckException("Could not reach GitHub. Check your internet connection.");
        }
        catch (TaskCanceledException) when (!cancel.IsCancellationRequested)
        {
            throw new UpdateCheckException("GitHub did not answer in time. Try again later.");
        }
    }
}
