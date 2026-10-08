namespace Clarion.Core.Cleanup;

/// <summary>Decides whether a folder may be cleaned. Used for every delete, not just at load time.</summary>
public static class PathGuard
{
    public static bool IsSafe(string path, ICleanupPlatform platform, bool trustedDerived = false)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (path.Contains("..", StringComparison.Ordinal)) return false;

        string full;
        try { full = Path.GetFullPath(path).TrimEnd('\\', '/'); }
        catch (ArgumentException) { return false; }
        catch (NotSupportedException) { return false; }

        if (full.Length <= 3) return false;
        if (Path.GetPathRoot(full)?.TrimEnd('\\', '/').Equals(full, StringComparison.OrdinalIgnoreCase) == true) return false;

        if (IsReparsePoint(full)) return false;
        if (trustedDerived) return true;

        if (platform.AllowedExact.Any(e => Normalize(e).Equals(full, StringComparison.OrdinalIgnoreCase))) return true;
        return platform.AllowedBases.Any(b =>
        {
            var baseFull = Normalize(b);
            return baseFull.Length > 3 && full.StartsWith(baseFull + "\\", StringComparison.OrdinalIgnoreCase);
        });
    }

    public static bool IsReparsePoint(string path)
    {
        try
        {
            return File.Exists(path) || Directory.Exists(path)
                ? (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0
                : false;
        }
        catch (IOException) { return true; }
        catch (UnauthorizedAccessException) { return true; }
    }

    private static string Normalize(string p) => Path.GetFullPath(p).TrimEnd('\\', '/');
}
