namespace Clarion.Core.Model;

/// <summary>Reads and changes text such as "VRROptimizeEnable=0;SwapEffectUpgradeEnable=1;", a list of Name=Value entries.</summary>
public static class RegistryList
{
    public static IReadOnlyList<(string Key, string Value)> Parse(string? text)
    {
        var entries = new List<(string, string)>();
        foreach (var part in (text ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var at = part.IndexOf('=');
            if (at > 0) entries.Add((part[..at].Trim(), part[(at + 1)..].Trim()));
        }
        return entries;
    }

    public static string? Get(string? text, string key) =>
        Parse(text).Where(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).Select(e => e.Value).FirstOrDefault();

    /// <summary>The list with the entry set. It keeps its place if it was there and goes on the end if not. Every entry ends with a semicolon.</summary>
    public static string Set(string? text, string key, string value)
    {
        var entries = Parse(text).ToList();
        var index = entries.FindIndex(e => e.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (index >= 0) entries[index] = (entries[index].Key, value);
        else entries.Add((key, value));
        // A second copy of the same name would make the first one decide, so only one is kept.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        return string.Concat(entries.Where(e => seen.Add(e.Key)).Select(e => $"{e.Key}={e.Value};"));
    }

    public static bool IsPlainName(string key) => key.Length is > 0 and <= 64 && key.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');

    public static bool IsPlainValue(string value) => value.Length is > 0 and <= 64 && value.All(c => char.IsAsciiLetterOrDigit(c) || c is '_' or '.' or '-');
}
