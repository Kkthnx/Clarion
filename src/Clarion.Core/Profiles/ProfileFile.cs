using System.Text.Json;
using System.Text.RegularExpressions;

namespace Clarion.Core.Profiles;

/// <summary>A saved setup: which settings should be on. Shareable as a small text file.</summary>
public sealed record SetupProfile(int Format, string App, string Version, DateTimeOffset Created, IReadOnlyList<string> Tweaks)
{
    public const int CurrentFormat = 1;
}

public sealed record ProfileLoad(SetupProfile? Profile, string? Error);

public static partial class ProfileFile
{
    public const long MaxBytes = 1_000_000;
    public const int MaxIds = 2000;

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    [GeneratedRegex(@"^[a-z0-9][a-z0-9.\-]{0,100}$")]
    private static partial Regex IdPattern();

    public static string Serialize(IEnumerable<string> tweakIds, string version) =>
        JsonSerializer.Serialize(new SetupProfile(SetupProfile.CurrentFormat, "Clarion", version, DateTimeOffset.UtcNow,
            tweakIds.Distinct(StringComparer.OrdinalIgnoreCase).Order(StringComparer.Ordinal).ToList()), Options);

    /// <summary>Reads a profile. A bad file never throws, it returns an error that is safe to show.</summary>
    public static ProfileLoad Parse(string json)
    {
        if (json.Length > MaxBytes) return new(null, "That file is too large to be a Clarion setup.");
        try
        {
            var profile = JsonSerializer.Deserialize<SetupProfile>(json, Options);
            if (profile is null || profile.App != "Clarion") return new(null, "That is not a Clarion setup file.");
            if (profile.Format != SetupProfile.CurrentFormat) return new(null, $"This setup file uses format {profile.Format}, which this version of Clarion cannot read.");
            if (profile.Tweaks is null) return new(null, "The setup file lists no settings.");
            if (profile.Tweaks.Count > MaxIds) return new(null, "The setup file lists too many settings.");
            if (profile.Tweaks.Any(id => id is null || !IdPattern().IsMatch(id))) return new(null, "The setup file contains an invalid setting name.");
            return new(profile, null);
        }
        catch (JsonException)
        {
            return new(null, "That file could not be read as a Clarion setup.");
        }
    }

    public static ProfileLoad Load(string path)
    {
        try
        {
            if (new FileInfo(path).Length > MaxBytes) return new(null, "That file is too large to be a Clarion setup.");
            return Parse(File.ReadAllText(path));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return new(null, $"The file could not be opened: {ex.Message}");
        }
    }
}
