using System.Text.Json;

namespace Clarion.Core.Drift;

/// <summary>Remembers the Windows build and time of the last scan, so the next one can tell a feature update happened.</summary>
public sealed class DriftState(string path)
{
    private sealed record Stored(int Build, DateTimeOffset ScannedAt);

    public int? LastBuild { get; private set; }
    public DateTimeOffset? LastScan { get; private set; }

    /// <summary>Reads the file. A missing or damaged file just means there is no earlier scan.</summary>
    public DriftState Load()
    {
        LastBuild = null;
        LastScan = null;
        try
        {
            if (!File.Exists(path)) return this;
            var s = JsonSerializer.Deserialize<Stored>(File.ReadAllText(path));
            if (s is { Build: > 0 }) { LastBuild = s.Build; LastScan = s.ScannedAt; }
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException) { }
        return this;
    }

    public void Save(int build, DateTimeOffset when)
    {
        LastBuild = build;
        LastScan = when;
        try
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonSerializer.Serialize(new Stored(build, when)));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
    }
}
