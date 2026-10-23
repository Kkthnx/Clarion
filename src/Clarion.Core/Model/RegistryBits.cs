using System.Globalization;

namespace Clarion.Core.Model;

/// <summary>Works on a value that holds flags, where a setting owns some of the bits and Windows may change the rest.</summary>
public static class RegistryBits
{
    public static bool TryParse(string text, out ulong value) =>
        ulong.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);

    /// <summary>Whether the value and the wanted value agree on every bit of the mask. False when any of them is not a plain number.</summary>
    public static bool Matches(string current, string wanted, string mask) =>
        TryParse(current, out var c) && TryParse(wanted, out var w) && TryParse(mask, out var m) && (c & m) == (w & m);

    /// <summary>The current value with the bits of the mask taken from the wanted value, or null when one of them is not a plain number.</summary>
    public static string? Merge(string current, string wanted, string mask) =>
        TryParse(current, out var c) && TryParse(wanted, out var w) && TryParse(mask, out var m)
            ? ((c & ~m) | (w & m)).ToString(CultureInfo.InvariantCulture)
            : null;

    /// <summary>Says which bits are set to what, for example "bit 0x4 to 0", so a reader does not have to work out the number.</summary>
    public static string Describe(string wanted, string mask)
    {
        if (!TryParse(wanted, out var w) || !TryParse(mask, out var m)) return $"the bits {mask} to {wanted}";
        var on = w & m;
        var off = m & ~w;
        var parts = new List<string>();
        if (off != 0) parts.Add($"{Bits(off)} off");
        if (on != 0) parts.Add($"{Bits(on)} on");
        return string.Join(" and ", parts);
    }

    private static string Bits(ulong bits) =>
        (System.Numerics.BitOperations.PopCount(bits) == 1 ? "bit " : "bits ") + $"0x{bits:X}";
}
