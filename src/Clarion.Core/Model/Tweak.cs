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
    public bool Supports(Requirements r) =>
        Build >= r.MinBuild &&
        (r.Editions.Count == 0 || r.Editions.Contains(Edition, StringComparer.OrdinalIgnoreCase));
}
