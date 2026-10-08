using System.Text.Json.Serialization;
using Clarion.Core.Model;

namespace Clarion.Core.Actions;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(RunProcess), "run")]
[JsonDerivedType(typeof(CleanFolders), "clean-folders")]
[JsonDerivedType(typeof(RenameFolder), "rename-folder")]
[JsonDerivedType(typeof(DeleteFolder), "delete-folder")]
public abstract record ActionStep
{
    public abstract string Describe();
}

/// <summary>Runs a Windows tool from the System32 folder. Only tools on the allow list can be named.</summary>
public sealed record RunProcess(string File, string Args = "", IReadOnlyList<int>? OkExitCodes = null, bool Utf16 = false, string? Label = null) : ActionStep
{
    public override string Describe() => Label ?? $"{File} {Args}".Trim();
}

/// <summary>Deletes files older than MinAgeHours inside the listed folders. Folders must be on the allow list.</summary>
public sealed record CleanFolders(IReadOnlyList<string> Folders, int MinAgeHours = 24) : ActionStep
{
    public override string Describe() => $"Clean files older than {MinAgeHours} hours in {string.Join(", ", Folders)}";
}

public sealed record RenameFolder(string Folder, string NewName) : ActionStep
{
    public override string Describe() => $"Rename {Folder} to {NewName}";
}

public sealed record DeleteFolder(string Folder) : ActionStep
{
    public override string Describe() => $"Delete {Folder}";
}

/// <summary>A one time job, such as a repair, that is not a setting and so has no on or off state.</summary>
public sealed record ActionDef
{
    public required string Id { get; init; }
    public string Category { get; init; } = "Repair";
    public required string Name { get; init; }
    public required string Summary { get; init; }
    public required string What { get; init; }
    public required string Benefit { get; init; }
    public required string Risk { get; init; }
    public required RiskLevel RiskLevel { get; init; }
    public Recommendation Recommendation { get; init; } = Recommendation.OnlyIf;
    public required string Advice { get; init; }
    public IReadOnlyList<string> Facts { get; init; } = [];
    public int EstimatedMinutes { get; init; } = 1;
    public bool NeedsReboot { get; init; }
    public bool RestorePoint { get; init; }
    public required IReadOnlyList<ActionStep> Steps { get; init; }
    public IReadOnlyList<string> Sources { get; init; } = [];
}
