using System.Net.Http.Headers;
using Clarion.Core.Updates;

namespace Clarion.Engine;

/// <summary>
/// Downloads the setup program of a newer release into .Clarion's own data folder, which only administrators can write to, so nothing else
/// on the PC can swap the file between the check and the start. The file is kept under a ".part" name until it is whole and its SHA-256
/// matches the one GitHub lists for it. A file that does not match is deleted and never started.
/// </summary>
public sealed class UpdateDownloader(string appVersion, string folder, HttpMessageHandler? handler = null)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);

    /// <summary>The setup program, checked, ready to start. Throws <see cref="UpdateCheckException"/> with a message safe to show.</summary>
    public async Task<string> DownloadSetupAsync(ReleaseInfo release, IProgress<double>? progress, CancellationToken cancel)
    {
        var setup = UpdateInstall.SetupOf(release)
            ?? throw new UpdateCheckException("This release has no setup program .Clarion can install by itself. Open the release page instead.");

#pragma warning disable CA2000 // HttpClient disposes the handler it is given.
        using var client = new HttpClient(handler ?? new HttpClientHandler { UseCookies = false }) { Timeout = Timeout };
#pragma warning restore CA2000
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Clarion", appVersion));

        try
        {
            Directory.CreateDirectory(folder);
            // Setup programs from earlier updates are not needed once a newer one is on its way.
            foreach (var old in Directory.EnumerateFiles(folder, "Clarion-*-setup.exe*"))
                try { File.Delete(old); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
        {
            throw new UpdateCheckException("The update could not be saved, because .Clarion is running without administrator rights. Start it as administrator and try again.");
        }

        var target = Path.Combine(folder, setup.Name);
        var part = target + ".part";
        try
        {
            var expected = setup.Sha256 ?? await ChecksumFromFileAsync(client, release, setup.Name, cancel)
                ?? throw new UpdateCheckException("GitHub lists no checksum for this setup program, so .Clarion will not start it. Open the release page instead.");

            using (var response = await client.GetAsync(setup.Url, HttpCompletionOption.ResponseHeadersRead, cancel))
            {
                if (!response.IsSuccessStatusCode)
                    throw new UpdateCheckException($"GitHub answered with an error ({(int)response.StatusCode}) while downloading. Try again later.");
                // A redirect may only lead to another HTTPS address. The checksum below is what proves the file, wherever it came from.
                if (response.RequestMessage?.RequestUri?.Scheme != Uri.UriSchemeHttps)
                    throw new UpdateCheckException("The download was sent to an address that is not secure, so it was stopped.");

                await using var source = await response.Content.ReadAsStreamAsync(cancel);
                await using var file = new FileStream(part, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                var buffer = new byte[81920];
                long done = 0;
                int read;
                while ((read = await source.ReadAsync(buffer, cancel)) > 0)
                {
                    done += read;
                    if (done > UpdateInstall.MaxSetupBytes || done > setup.Size)
                        throw new UpdateCheckException("The download is larger than the release says it should be, so it was stopped.");
                    await file.WriteAsync(buffer.AsMemory(0, read), cancel);
                    progress?.Report((double)done / setup.Size);
                }
                if (done != setup.Size)
                    throw new UpdateCheckException("The download ended early. Try again.");
            }

            var actual = UpdateInstall.Sha256Of(part);
            if (!string.Equals(actual, expected, StringComparison.Ordinal))
                throw new UpdateCheckException("The downloaded setup program does not match the checksum GitHub lists for it, so it was deleted and not started.");

            File.Move(part, target, overwrite: true);
            return target;
        }
        catch (HttpRequestException)
        {
            throw new UpdateCheckException("Could not reach GitHub to download the update. Check your internet connection.");
        }
        catch (TaskCanceledException) when (!cancel.IsCancellationRequested)
        {
            throw new UpdateCheckException("The download took too long and was stopped. Try again later.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new UpdateCheckException($"The update could not be saved: {ex.Message}");
        }
        finally
        {
            try { if (File.Exists(part)) File.Delete(part); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    private static async Task<string?> ChecksumFromFileAsync(HttpClient client, ReleaseInfo release, string setupName, CancellationToken cancel)
    {
        if (UpdateInstall.ChecksumOf(release) is not { } file) return null;
        var text = await client.GetStringAsync(file.Url, cancel);
        return UpdateInstall.ParseChecksumFile(text, setupName);
    }
}
