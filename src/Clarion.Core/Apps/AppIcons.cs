using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clarion.Core.Apps;

public enum LogoShape { Square, Wide, Wordmark }

/// <summary>One brand mark: the outline as SVG path data on a 24 by 24 grid, the brand's own colour, and its width over height.</summary>
public sealed record AppIcon(string Title, string Hex, string Source, string Path, double Aspect = 1.0)
{
    /// <summary>
    /// How the outline sits in a square slot. A wide outline such as a wordmark is drawn inside a tile so it is not a thin sliver,
    /// and a very thin one is not readable at icon size at all.
    /// </summary>
    public LogoShape Shape => Aspect >= 2.2 ? LogoShape.Wordmark : Aspect >= 1.5 ? LogoShape.Wide : LogoShape.Square;

    /// <summary>Red, green and blue as 0 to 255, from the brand's six digit hex colour.</summary>
    public (byte R, byte G, byte B) Rgb
    {
        get
        {
            var v = int.Parse(Hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return ((byte)(v >> 16), (byte)(v >> 8), (byte)v);
        }
    }

    /// <summary>How bright the colour looks, 0 for black to 1 for white (WCAG relative luminance).</summary>
    public double Luminance
    {
        get
        {
            static double Channel(byte c)
            {
                var s = c / 255.0;
                return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
            }
            var (r, g, b) = Rgb;
            return 0.2126 * Channel(r) + 0.7152 * Channel(g) + 0.0722 * Channel(b);
        }
    }

    /// <summary>False when the colour would nearly vanish on the given background, so the caller should use the text colour instead.</summary>
    public bool ReadableOn(bool darkBackground) => darkBackground ? Luminance >= 0.12 : Luminance <= 0.75;
}

/// <summary>The logos shown beside apps. They come from the Simple Icons set, which also lists where each one was taken from.</summary>
public sealed class AppIcons
{
    private static readonly Lazy<AppIcons> Embedded = new(LoadEmbedded);

    private readonly Dictionary<string, AppIcon> _icons;

    private AppIcons(string set, string setUrl, Dictionary<string, AppIcon> icons)
    {
        Set = set;
        SetUrl = setUrl;
        _icons = icons;
    }

    public static AppIcons Default => Embedded.Value;

    /// <summary>The name and version of the icon set, for the credits.</summary>
    public string Set { get; }

    public string SetUrl { get; }

    public IReadOnlyDictionary<string, AppIcon> All => _icons;

    /// <summary>The icon for a catalog id, or null when there is none and a plain tile is shown instead.</summary>
    public AppIcon? For(string tweakId) => _icons.GetValueOrDefault(tweakId);

    public static AppIcons Parse(Stream json)
    {
        var file = JsonSerializer.Deserialize<IconFile>(json, Catalog.CatalogLoader.Options) ?? throw new InvalidDataException("Empty icon file");
        // Every outline is made absolute here, once, so the drawing code never has to agree with SVG about what a relative move means.
        var icons = file.Icons.ToDictionary(p => p.Key, p => p.Value with { Path = SvgPath.ToAbsolute(p.Value.Path) }, StringComparer.Ordinal);
        return new AppIcons(file.Set, file.SetUrl, icons);
    }

    private static AppIcons LoadEmbedded()
    {
        var asm = typeof(AppIcons).Assembly;
        var name = asm.GetManifestResourceNames().Single(n => n.EndsWith(".Catalog.Icons.app-icons.json", StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(name)!;
        return Parse(stream);
    }

    private sealed record IconFile(
        [property: JsonRequired] string Set,
        [property: JsonRequired] string SetUrl,
        [property: JsonRequired] Dictionary<string, AppIcon> Icons);
}
