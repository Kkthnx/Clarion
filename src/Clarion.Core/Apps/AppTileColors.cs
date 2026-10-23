namespace Clarion.Core.Apps;

/// <summary>
/// The colours of the tile that stands in for a logo, worked out from the app's name alone. The same name always gets the same tint, on every
/// page and every run, and the tint is soft enough that a row of them reads as one family and not a set of missing pictures.
/// </summary>
public static class AppTileColors
{
    /// <summary>The character shown on the tile: the first letter or digit of the name, in capitals. Dots, dashes and spaces in front are skipped.</summary>
    public static string LetterOf(string name)
    {
        foreach (var c in name)
            if (char.IsLetterOrDigit(c)) return char.ToUpperInvariant(c).ToString();
        return "";
    }

    /// <summary>The tile colour and the letter colour, each as red, green and blue from 0 to 255.</summary>
    public static ((byte R, byte G, byte B) Tile, (byte R, byte G, byte B) Ink) For(string name, bool dark)
    {
        var hue = Hue(name);
        return dark
            ? (Hsl(hue, 0.30, 0.22), Hsl(hue, 0.65, 0.78))
            : (Hsl(hue, 0.45, 0.90), Hsl(hue, 0.65, 0.24));
    }

    /// <summary>A hue from 0 to 360 that does not change from run to run. string.GetHashCode is different in every process, so it is not used.</summary>
    public static double Hue(string name)
    {
        // FNV-1a over the letters of the name in lower case, so "Git" and "git" agree.
        var hash = 2166136261u;
        foreach (var c in name.ToLowerInvariant())
            hash = (hash ^ c) * 16777619u;
        return hash % 360;
    }

    private static (byte R, byte G, byte B) Hsl(double hue, double saturation, double lightness)
    {
        var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
        var m = lightness - c / 2;
        var (r, g, b) = (int)(hue / 60) switch
        {
            0 => (c, x, 0.0),
            1 => (x, c, 0.0),
            2 => (0.0, c, x),
            3 => (0.0, x, c),
            4 => (x, 0.0, c),
            _ => (c, 0.0, x),
        };
        return ((byte)Math.Round((r + m) * 255), (byte)Math.Round((g + m) * 255), (byte)Math.Round((b + m) * 255));
    }
}
