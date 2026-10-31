namespace Clarion.Core.Model;

public sealed record Requirements
{
    public int MinBuild { get; init; }
    public IReadOnlyList<string> Editions { get; init; } = [];
}

public sealed record Tweak
{
    public required string Id { get; init; }
    public required string Category { get; init; }

    /// <summary>A smaller group inside the category, such as Search or App permissions.</summary>
    public string Topic { get; init; } = "";
    public required string Name { get; init; }
    public required string Summary { get; init; }
    public required string What { get; init; }
    public required string Benefit { get; init; }
    public required string Risk { get; init; }
    public required Evidence Evidence { get; init; }
    public required RiskLevel RiskLevel { get; init; }
    /// <summary>Tweaks in the same group are choices of one setting, so only one can be on.</summary>
    public string? ExclusiveGroup { get; init; }
    public Recommendation Recommendation { get; init; } = Recommendation.Optional;
    public string Advice { get; init; } = "";
    public IReadOnlyList<string> Facts { get; init; } = [];
    public TweakScope Scope { get; init; } = TweakScope.User;
    public Requirements Requires { get; init; } = new();

    /// <summary>
    /// How the setting sits on a portable PC. Caution is shown as a warning there, and Avoid is also left out of presets and setup files
    /// unless the person asks for it. A desktop is never affected.
    /// </summary>
    public PortableFit Portable { get; init; } = PortableFit.Any;

    /// <summary>Why a laptop needs care with this setting, in plain words. Required whenever <see cref="Portable"/> is not Any.</summary>
    public string PortableNote { get; init; } = "";

    /// <summary>The Windows build this setting was last checked on. 0 means never checked.</summary>
    public int LastVerifiedBuild { get; init; }
    public bool NeedsReboot { get; init; }
    public bool NeedsSignOut { get; init; }
    public bool RestartsExplorer { get; init; }
    public required IReadOnlyList<Operation> Apply { get; init; }
    public IReadOnlyList<Operation> Undo { get; init; } = [];
    public IReadOnlyList<string> Sources { get; init; } = [];

    /// <summary>The exact Microsoft policy definitions this setting is built from, for people who want to check.</summary>
    public IReadOnlyList<string> Provenance { get; init; } = [];
}

public sealed record MachineProfile(int Build, string Edition)
{
    /// <summary>Whether this is a laptop or a desktop, and what graphics it has. A desktop until it is known otherwise.</summary>
    public MachineTraits Traits { get; init; } = MachineTraits.Desktop;

    /// <summary>The warning for a setting that needs care on a laptop, or null on a desktop and for a setting with nothing to say.</summary>
    public string? PortableWarning(Tweak t) =>
        Traits.IsPortable && t.Portable != PortableFit.Any && t.PortableNote.Length > 0 ? t.PortableNote : null;

    /// <summary>True when a laptop should not get this setting unless the person asks for it by name.</summary>
    public bool HoldsBack(Tweak t) => Traits.IsPortable && t.Portable == PortableFit.Avoid;

    public bool Supports(Requirements r) =>
        Build >= r.MinBuild &&
        (r.Editions.Count == 0 || r.Editions.Contains(Edition, StringComparer.OrdinalIgnoreCase));

    /// <summary>Why a setting with these requirements cannot be used on this PC, in plain words, or null when it can.</summary>
    public string? WhyNotSupported(Requirements r)
    {
        if (Build < r.MinBuild)
            return r.MinBuild >= 22000 ? "Needs Windows 11" : $"Needs Windows build {r.MinBuild} or later";
        if (Supports(r)) return null;
        return $"Not supported on {Edition}. Works on {string.Join(", ", r.Editions)}";
    }
}
