using System.Text.Json;

namespace Clarion.Core.Settings;

/// <summary>The choices Clarion remembers between runs. Nothing here leaves the PC.</summary>
public sealed record UserSettings
{
    /// <summary>System, Light or Dark.</summary>
    public string Theme { get; init; } = "System";

    public bool ExpertMode { get; init; }

    /// <summary>The settings lists show one line per setting, so more fit on the screen.</summary>
    public bool CompactLists { get; init; }

    /// <summary>The welcome and preset choice has been shown, so it is not shown again.</summary>
    public bool FirstRunDone { get; init; }

    /// <summary>Look for a newer version when Clarion starts. Off unless the person turns it on, because it contacts GitHub.</summary>
    public bool CheckForUpdates { get; init; }

    public DateTimeOffset? LastUpdateCheck { get; init; }

    /// <summary>A newer version the person chose to hide, so it is not offered again until there is an even newer one.</summary>
    public string? DismissedUpdate { get; init; }

    /// <summary>The version that was running the last time the person saw the "what changed" notice. A newer one running now means Clarion was updated.</summary>
    public string? LastSeenVersion { get; init; }

    /// <summary>A scheduled task runs a read only check after Windows Update's monthly patch day.</summary>
    public bool MonthlyVerify { get; init; }

    public static readonly string[] Themes = ["System", "Light", "Dark"];

    /// <summary>
    /// The welcome is for someone who has never applied anything. A person who already has settings applied, such as someone who
    /// had Clarion before this was added, is not welcomed, however empty their saved choices are.
    /// </summary>
    public bool NeedsWelcome(int settingsAlreadyApplied) => !FirstRunDone && settingsAlreadyApplied == 0;

    /// <summary>A name that is not one of the three is treated as System, so a hand edited file cannot break startup.</summary>
    public UserSettings Cleaned() => Themes.Contains(Theme) ? this : this with { Theme = "System" };
}

/// <summary>Reads and writes <see cref="UserSettings"/> as a small JSON file.</summary>
public sealed class UserSettingsStore(string path)
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly object _gate = new();

    /// <summary>A missing or damaged file gives the defaults. It is never an error.</summary>
    public UserSettings Load()
    {
        lock (_gate)
        {
            try
            {
                if (!File.Exists(path)) return new UserSettings();
                return (JsonSerializer.Deserialize<UserSettings>(File.ReadAllText(path), Options) ?? new UserSettings()).Cleaned();
            }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException) { return new UserSettings(); }
        }
    }

    /// <summary>
    /// Writes a temporary file and swaps it in, so a power cut or a crash in the middle leaves the earlier file whole.
    /// Returns false, and changes nothing, when the file cannot be written.
    /// </summary>
    public bool Save(UserSettings settings)
    {
        lock (_gate)
        {
            try
            {
                var dir = Path.GetDirectoryName(Path.GetFullPath(path));
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                var temp = path + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(settings.Cleaned(), Options));
                File.Move(temp, path, overwrite: true);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return false; }
        }
    }
}
