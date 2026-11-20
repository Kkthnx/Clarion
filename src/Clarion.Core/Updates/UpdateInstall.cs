using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Clarion.Core.Updates;

/// <summary>One file attached to a release: its name, where to download it, its size, and its SHA-256 when GitHub lists one.</summary>
public sealed record ReleaseAsset(string Name, string Url, long Size, string? Sha256);

/// <summary>
/// The rules for updating from inside .Clarion. Only the setup program of this project's own release is taken, only from this project's
/// download address, and only when its SHA-256 is known beforehand and the downloaded file matches it. Anything else falls back to
/// opening the release page, as before.
/// </summary>
public static partial class UpdateInstall
{
    /// <summary>A release file larger than this is refused. The setup program is about 60 MB.</summary>
    public const long MaxSetupBytes = 400L * 1024 * 1024;

    /// <summary>The Inno Setup id of .Clarion, as written under the Uninstall key with "_is1" after it.</summary>
    public const string InstallerKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{6F0B2C1E-4D7A-4B9E-9C55-2E1A7C0D8F31}_is1";

    [GeneratedRegex("^[0-9a-f]{64}$")]
    private static partial Regex Hex64();

    /// <summary>The file name the release workflow gives the setup program of a version, such as Clarion-1.8.0-setup.exe.</summary>
    public static string SetupName(string tag) => $"Clarion-{(tag.StartsWith('v') || tag.StartsWith('V') ? tag[1..] : tag)}-setup.exe";

    /// <summary>True only for a file in a release of this project, downloaded over HTTPS from GitHub.</summary>
    public static bool IsTrustedDownloadUrl(string? url, string tag) =>
        Uri.TryCreate(url, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps &&
        u.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) &&
        u.AbsolutePath.StartsWith($"/{UpdateCheck.Repository}/releases/download/{tag}/", StringComparison.OrdinalIgnoreCase);

    /// <summary>Reads GitHub's "sha256:..." digest into lower case hex, or null for anything else.</summary>
    public static string? ParseDigest(string? digest)
    {
        if (digest is null || !digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase)) return null;
        var hex = digest[7..].Trim().ToLowerInvariant();
        return Hex64().IsMatch(hex) ? hex : null;
    }

    /// <summary>Reads a "hash  name" line from the .sha256 file the release carries, for the named file. Null when it does not say.</summary>
    public static string? ParseChecksumFile(string text, string fileName)
    {
        foreach (var line in text.Split('\n'))
        {
            var parts = line.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 0) continue;
            var hex = parts[0].ToLowerInvariant();
            if (!Hex64().IsMatch(hex)) continue;
            // A line with no name is accepted, since the file is named after the one it covers.
            if (parts.Length == 1 || parts[1].TrimStart('*').Equals(fileName, StringComparison.OrdinalIgnoreCase)) return hex;
        }
        return null;
    }

    /// <summary>The setup program of a release, when it is there under its expected name, from a trusted address, and not too large.</summary>
    public static ReleaseAsset? SetupOf(ReleaseInfo release)
    {
        var name = SetupName(release.Tag);
        var setup = release.Assets?.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return setup is not null && IsTrustedDownloadUrl(setup.Url, release.Tag) && setup.Size is > 0 and <= MaxSetupBytes ? setup : null;
    }

    /// <summary>The .sha256 file that goes with the setup program, for a release where GitHub lists no digest.</summary>
    public static ReleaseAsset? ChecksumOf(ReleaseInfo release)
    {
        var name = SetupName(release.Tag) + ".sha256";
        var file = release.Assets?.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return file is not null && IsTrustedDownloadUrl(file.Url, release.Tag) && file.Size is > 0 and < 4096 ? file : null;
    }

    /// <summary>The SHA-256 of a file, in lower case hex.</summary>
    public static string Sha256Of(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    /// <summary>
    /// What the setup program is started with. /SILENT shows its progress window and asks nothing, /SUPPRESSMSGBOXES answers any message
    /// with its default, /NORESTART never restarts Windows, and /RELAUNCH starts .Clarion again when it is done (see installer\Clarion.iss).
    /// The log goes beside the download, so a failed update can be read afterwards.
    /// </summary>
    public static string SetupArguments(string logPath) => $"/SILENT /SUPPRESSMSGBOXES /NORESTART /RELAUNCH /LOG=\"{logPath}\"";

    /// <summary>
    /// True when this copy is the one the setup program installed, so running a newer setup program replaces it. A portable copy, or one
    /// started from a build folder, would be left as it is while another copy was installed, so it is sent to the release page instead.
    /// </summary>
    public static bool IsInstalledCopy(string runningFolder, string? installLocation)
    {
        if (string.IsNullOrWhiteSpace(installLocation)) return false;
        static string Norm(string p) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(p));
        try { return string.Equals(Norm(runningFolder), Norm(installLocation), StringComparison.OrdinalIgnoreCase); }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException) { return false; }
    }
}
