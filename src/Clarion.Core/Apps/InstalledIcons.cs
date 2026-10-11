using System.Text.Json;

namespace Clarion.Core.Apps;

/// <summary>
/// The Uninstall registry codes winget's own package records list for each app, and so the keys under
/// <c>SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall</c> that show where the app is installed.
/// </summary>
public sealed class AppProducts
{
    private static readonly Lazy<AppProducts> Embedded = new(LoadEmbedded);

    private readonly Dictionary<string, string[]> _codes;

    private AppProducts(Dictionary<string, string[]> codes) => _codes = codes;

    public static AppProducts Default => Embedded.Value;

    public IReadOnlyDictionary<string, string[]> All => _codes;

    /// <summary>The registry codes for a catalog id, or none when the package record names none.</summary>
    public IReadOnlyList<string> For(string tweakId) => _codes.TryGetValue(tweakId, out var codes) ? codes : [];

    public static AppProducts Parse(Stream json) =>
        new(JsonSerializer.Deserialize<Dictionary<string, string[]>>(json) ?? throw new InvalidDataException("Empty product file"));

    private static AppProducts LoadEmbedded()
    {
        var asm = typeof(AppProducts).Assembly;
        var name = asm.GetManifestResourceNames().Single(n => n.EndsWith(".Catalog.Icons.app-products.json", StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(name)!;
        return Parse(stream);
    }
}

/// <summary>Reads the DisplayIcon value of an Uninstall key, the file Windows itself shows as the program's icon in Settings.</summary>
public static class DisplayIcons
{
    /// <summary>
    /// The icon file a DisplayIcon value names, or null when it is empty, not a plain file on a local drive, or not an exe or ico file.
    /// The value may be quoted and may end in a comma and an icon number. A network path or a device path is refused, so reading the
    /// icon cannot reach out to another machine.
    /// </summary>
    public static string? ResolveFile(string? displayIcon, Func<string, string>? expand = null)
    {
        if (string.IsNullOrWhiteSpace(displayIcon)) return null;
        var text = (expand ?? Environment.ExpandEnvironmentVariables)(displayIcon.Trim());

        if (text.StartsWith('"'))
        {
            var end = text.IndexOf('"', 1);
            if (end < 0) return null;
            text = text[1..end];
        }
        else
        {
            var comma = text.LastIndexOf(',');
            if (comma > 1 && int.TryParse(text[(comma + 1)..].Trim(), out _)) text = text[..comma];
        }

        text = text.Trim();
        if (text.Length < 5 || !char.IsAsciiLetter(text[0]) || text[1] != ':' || text[2] != '\\') return null;
        if (text.Split('\\').Contains("..")) return null;
        var extension = Path.GetExtension(text);
        return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) || extension.Equals(".ico", StringComparison.OrdinalIgnoreCase) ? text : null;
    }
}
