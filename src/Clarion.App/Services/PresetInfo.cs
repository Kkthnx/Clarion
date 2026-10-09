namespace Clarion.App.Services;

/// <summary>The names and plain descriptions of the presets, used on Home and in the first run welcome.</summary>
public static class PresetInfo
{
    public static readonly (string Key, string Title, string Text)[] All =
    [
        ("minimal", "Minimal", "Stop promotions and drop diagnostic data to the lowest level your edition allows. The gentlest choice."),
        ("standard", "Standard", "Minimal plus private search, no activity history, no update sharing and a few Explorer basics. Best for most people."),
        ("advanced", "Advanced", "Standard plus telemetry services and tasks, location off, classic right-click menu and tidier taskbar."),
        ("privacy", "Privacy max", "Every privacy and promotion setting we suggest for everyone. Only low risk items, no app permission lockdowns."),
        ("gaming", "Gaming", "Pointer, key and menu tweaks, fewer background jobs and no update sharing."),
    ];
}
