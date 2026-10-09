using System.Text.Json;

namespace Clarion.Core.Updates;

/// <summary>A version number as written on a release, such as 0.1.0-beta.4. Compared the way semantic versioning says.</summary>
public sealed record ReleaseVersion(int Major, int Minor, int Patch, IReadOnlyList<string> PreRelease) : IComparable<ReleaseVersion>
{
    public bool IsPreRelease => PreRelease.Count > 0;

    /// <summary>Accepts a leading v and ignores build details after a plus sign. Returns null for anything else.</summary>
    public static ReleaseVersion? Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var t = text.Trim();
        if (t.StartsWith('v') || t.StartsWith('V')) t = t[1..];
        var plus = t.IndexOf('+');
        if (plus >= 0) t = t[..plus];

        var dash = t.IndexOf('-');
        var core = dash >= 0 ? t[..dash] : t;
        var pre = dash >= 0 ? t[(dash + 1)..].Split('.') : [];

        var parts = core.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var major) || !int.TryParse(parts[1], out var minor) || !int.TryParse(parts[2], out var patch)) return null;
        if (major < 0 || minor < 0 || patch < 0 || pre.Any(string.IsNullOrEmpty)) return null;
        return new ReleaseVersion(major, minor, patch, pre);
    }

    public int CompareTo(ReleaseVersion? other)
    {
        if (other is null) return 1;
        var core = (Major, Minor, Patch).CompareTo((other.Major, other.Minor, other.Patch));
        if (core != 0) return core;

        // A release with no pre-release part comes after the same version with one.
        if (!IsPreRelease && !other.IsPreRelease) return 0;
        if (!IsPreRelease) return 1;
        if (!other.IsPreRelease) return -1;

        for (var i = 0; i < Math.Min(PreRelease.Count, other.PreRelease.Count); i++)
        {
            var a = PreRelease[i];
            var b = other.PreRelease[i];
            var aNumber = int.TryParse(a, out var an);
            var bNumber = int.TryParse(b, out var bn);
            // Numbers compare as numbers, so beta.10 follows beta.9. Words compare as text, and numbers come before words.
            var c = aNumber && bNumber ? an.CompareTo(bn) : aNumber ? -1 : bNumber ? 1 : string.CompareOrdinal(a, b);
            if (c != 0) return c;
        }
        return PreRelease.Count.CompareTo(other.PreRelease.Count);
    }

    public static bool operator >(ReleaseVersion a, ReleaseVersion b) => a.CompareTo(b) > 0;
    public static bool operator <(ReleaseVersion a, ReleaseVersion b) => a.CompareTo(b) < 0;
    public static bool operator >=(ReleaseVersion a, ReleaseVersion b) => a.CompareTo(b) >= 0;
    public static bool operator <=(ReleaseVersion a, ReleaseVersion b) => a.CompareTo(b) <= 0;
}

public sealed record ReleaseInfo(string Tag, string Name, string Url, bool PreRelease, bool Draft, DateTimeOffset? Published, string Body = "");

/// <summary>A newer version that is on offer. Clarion only points to the page. It never downloads or installs anything.</summary>
public sealed record UpdateOffer(string Tag, string Name, string Url, bool PreRelease);

/// <summary>Where the list of releases comes from. A fake stands in for it in tests.</summary>
public interface IReleaseSource
{
    /// <summary>The releases as JSON. Throws <see cref="UpdateCheckException"/> with a message that is safe to show.</summary>
    Task<string> GetReleasesJsonAsync(CancellationToken cancel);
}

public sealed class UpdateCheckException(string message) : Exception(message);

public static class UpdateCheck
{
    public const string Repository = "Kkthnx/Clarion";

    /// <summary>Links are only opened when they point at this project's releases, whatever the answer from the network said.</summary>
    public static bool IsTrustedReleaseUrl(string? url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps &&
        u.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase) &&
        u.AbsolutePath.StartsWith($"/{Repository}/releases", StringComparison.OrdinalIgnoreCase);

    public static IReadOnlyList<ReleaseInfo> ParseReleases(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Array) return [];
            var list = new List<ReleaseInfo>();
            foreach (var e in doc.RootElement.EnumerateArray())
            {
                if (e.ValueKind != JsonValueKind.Object) continue;
                var tag = Text(e, "tag_name");
                if (tag.Length == 0) continue;
                list.Add(new ReleaseInfo(
                    tag, Text(e, "name"), Text(e, "html_url"),
                    Flag(e, "prerelease"), Flag(e, "draft"),
                    DateTimeOffset.TryParse(Text(e, "published_at"), out var when) ? when : null,
                    Text(e, "body")));
            }
            return list;
        }
        catch (JsonException) { return []; }
    }

    private static string Text(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

    private static bool Flag(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;

    /// <summary>
    /// The newest release that is newer than the one running, or null. Drafts and pages that are not this project's are ignored.
    /// Someone on a beta is offered betas and final releases. Someone on a final release is only offered final ones.
    /// </summary>
    public static UpdateOffer? Newest(string currentVersion, IEnumerable<ReleaseInfo> releases)
    {
        var newest = NewerThan(currentVersion, releases).FirstOrDefault();
        return newest is null ? null : new UpdateOffer(newest.Tag, string.IsNullOrWhiteSpace(newest.Name) ? newest.Tag : newest.Name, newest.Url, newest.PreRelease);
    }

    /// <summary>
    /// Every release newer than the one running, newest first, by the same rules as <see cref="Newest"/>. Someone who skipped a version
    /// sees what they missed as well as what is latest.
    /// </summary>
    public static IReadOnlyList<ReleaseInfo> NewerThan(string currentVersion, IEnumerable<ReleaseInfo> releases)
    {
        var current = ReleaseVersion.Parse(currentVersion);
        if (current is null) return [];

        return releases
            .Where(r => !r.Draft && IsTrustedReleaseUrl(r.Url))
            .Select(r => (Release: r, Version: ReleaseVersion.Parse(r.Tag)))
            .Where(x => x.Version is not null && (current.IsPreRelease || !x.Version.IsPreRelease))
            .Where(x => x.Version! > current)
            .OrderByDescending(x => x.Version, Comparer<ReleaseVersion?>.Create((a, b) => a!.CompareTo(b)))
            .Select(x => x.Release)
            .ToList();
    }

    /// <summary>A version the person hid is not offered again. A newer one than that is.</summary>
    public static bool ShouldShow(UpdateOffer offer, string? dismissedTag)
    {
        if (string.IsNullOrWhiteSpace(dismissedTag)) return true;
        var hidden = ReleaseVersion.Parse(dismissedTag);
        var offered = ReleaseVersion.Parse(offer.Tag);
        return hidden is null || offered is null || offered > hidden;
    }

    public static async Task<UpdateOffer?> RunAsync(IReleaseSource source, string currentVersion, CancellationToken cancel = default) =>
        Newest(currentVersion, ParseReleases(await source.GetReleasesJsonAsync(cancel)));
}
