namespace Clarion.Core.SystemInfo;

/// <summary>What was observed on the machine. Plain data so the judgement below can be tested.</summary>
public sealed record InstallInputs
{
    public IReadOnlyList<string> Branding { get; init; } = [];
    public bool HardwareChecksBypassed { get; init; }
    public bool DefenderServiceMissing { get; init; }
    public bool UpdateServiceMissing { get; init; }
    public bool UpdateServiceDisabled { get; init; }
    public IReadOnlyList<string> MissingServices { get; init; } = [];
    public bool? StoreMissing { get; init; }
    public bool? EdgeMissing { get; init; }
    public string? LicenseChannel { get; init; }
    public bool Domain { get; init; }
}

/// <summary>
/// Weighs known signs of a stripped or rebranded Windows image. Windows has no flag for this, so
/// every sign is listed with its reason and the result is a hint, never a claim.
/// </summary>
public static class InstallCheck
{
    /// <summary>Services that a standard install always has. Missing ones were removed, not just turned off.</summary>
    public static readonly IReadOnlyList<string> CoreServices =
        ["DiagTrack", "WerSvc", "wscsvc", "SecurityHealthService", "UsoSvc", "BITS", "SysMain", "WSearch"];

    private static readonly string[] KnownImageNames = ["ghost spectre", "tiny11", "tiny10", "atlasos", "revios"];

    private const int LikelyAt = 5;
    private const int PossibleAt = 2;

    public static InstallVerdict Evaluate(InstallInputs i)
    {
        var signals = new List<InstallSignal>();

        var named = i.Branding.FirstOrDefault(b => KnownImageNames.Any(k => b.Contains(k, StringComparison.OrdinalIgnoreCase)));
        if (named is not null) signals.Add(new($"The system information names a custom Windows image: \"{named.Trim()}\"", 5));

        if (i.HardwareChecksBypassed)
            signals.Add(new("Setup was told to skip the hardware checks (TPM, Secure Boot, RAM or CPU)", 2));
        if (i.UpdateServiceMissing)
            signals.Add(new("The Windows Update service is not installed", 3));
        else if (i.UpdateServiceDisabled)
            signals.Add(new("The Windows Update service is disabled", 1));
        if (i.DefenderServiceMissing)
            signals.Add(new("The Microsoft Defender service is not installed", 2));

        var missing = i.MissingServices.Where(s => CoreServices.Contains(s, StringComparer.OrdinalIgnoreCase)).ToList();
        if (missing.Count >= 3)
            signals.Add(new($"{missing.Count} standard services are not installed: {string.Join(", ", missing)}", 2));

        if (i.StoreMissing == true) signals.Add(new("The Microsoft Store is not installed", 1));
        if (i.EdgeMissing == true) signals.Add(new("Microsoft Edge is not installed", 1));
        if (!i.Domain && string.Equals(i.LicenseChannel, "Volume:GVLK", StringComparison.OrdinalIgnoreCase))
            signals.Add(new("Windows is activated with a volume license key on a PC that is not on a domain", 1));

        var score = signals.Sum(s => s.Weight);
        var level = score >= LikelyAt ? InstallLevel.Likely : score >= PossibleAt ? InstallLevel.Possible : InstallLevel.Standard;
        return new InstallVerdict(level, signals);
    }
}
