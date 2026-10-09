namespace Clarion.Core.Catalog;

/// <summary>
/// Staleness rules for the catalog. Every tweak records the Windows build it was last checked on.
/// When Microsoft ships a newer release, raise <see cref="NewestKnownBuild"/> and re-check the
/// entries that fall outside the window; the tests fail until that is done.
/// </summary>
public static class CatalogFreshness
{
    /// <summary>The newest Windows build the catalog has been reviewed against.</summary>
    public const int NewestKnownBuild = 26200;

    /// <summary>Entries last verified before this build count as stale (Windows 11 24H2).</summary>
    public const int OldestAcceptableBuild = 26100;
}
