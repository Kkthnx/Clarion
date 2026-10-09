using System.Text.Json.Serialization;
using Clarion.Core.Model;

namespace Clarion.Core.Cleanup;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(FolderRule), "folder")]
[JsonDerivedType(typeof(FilePatternRule), "files")]
[JsonDerivedType(typeof(SteamShaderRule), "steam-shaders")]
[JsonDerivedType(typeof(WowCacheRule), "wow-cache")]
[JsonDerivedType(typeof(RecycleBinRule), "recycle-bin")]
[JsonDerivedType(typeof(EventLogsRule), "event-logs")]
public abstract record CleanRule;

/// <summary>Everything inside a folder. The folder itself is kept.</summary>
public sealed record FolderRule(string Path) : CleanRule;

/// <summary>Only files in a folder whose names match a pattern such as thumbcache_*.db.</summary>
public sealed record FilePatternRule(string Folder, string Pattern) : CleanRule;

/// <summary>The steamapps\shadercache folder in every Steam library.</summary>
public sealed record SteamShaderRule : CleanRule;

/// <summary>The Cache folder of each installed World of Warcraft version.</summary>
public sealed record WowCacheRule : CleanRule;

public sealed record RecycleBinRule : CleanRule;

/// <summary>Clears Windows event logs. The Security log is never cleared.</summary>
public sealed record EventLogsRule : CleanRule;

/// <summary>One row in the cleanup list, such as NVIDIA shader caches or Discord cache.</summary>
public sealed record CleanTarget
{
    public required string Id { get; init; }
    public required string Group { get; init; }
    public required string Name { get; init; }
    public required string Summary { get; init; }
    public required string Advice { get; init; }
    public Recommendation Recommendation { get; init; } = Recommendation.Optional;
    public IReadOnlyList<string> Facts { get; init; } = [];
    public required string Benefit { get; init; }
    public required string Risk { get; init; }
    public required RiskLevel RiskLevel { get; init; }

    /// <summary>Starts ticked in the list.</summary>
    public bool DefaultOn { get; init; }

    /// <summary>Cannot be undone, for example emptying the Recycle Bin.</summary>
    public bool Irreversible { get; init; }

    /// <summary>Only shown, and only run from the window, in Expert mode.</summary>
    public bool ExpertOnly { get; init; }

    /// <summary>Process names that keep these files open. The row is skipped while one is running.</summary>
    public IReadOnlyList<string> Processes { get; init; } = [];

    /// <summary>Only files untouched for this many hours are removed. Zero removes everything.</summary>
    public int MinAgeHours { get; init; }

    /// <summary>Files held open by the driver can be queued for deletion at the next restart.</summary>
    public bool QueueLockedAtRestart { get; init; }

    public required IReadOnlyList<CleanRule> Rules { get; init; }
    public IReadOnlyList<string> Sources { get; init; } = [];
}

public sealed record RuleScan(string Folder, long Bytes, int Files, bool Exists);

public sealed record TargetScan(string TargetId, IReadOnlyList<RuleScan> Folders, long Bytes, int Files, IReadOnlyList<string> RunningApps)
{
    public bool Present => Folders.Any(f => f.Exists) || Bytes > 0;
}

public sealed record TargetClean(
    string TargetId,
    long BytesFreed,
    int FilesDeleted,
    int FilesLocked,
    int QueuedForRestart,
    IReadOnlyList<string> Notes,
    bool Skipped = false)
{
    /// <summary>Files set to be removed at the next restart. In a preview, the files that would be.</summary>
    public IReadOnlyList<string> QueuedFiles { get; init; } = [];
}

public interface ICleanupPlatform
{
    /// <summary>Expands environment variables and the %LOCALLOW% token.</summary>
    string Expand(string template);

    /// <summary>Folders cleanup may work in. Anything outside them is refused.</summary>
    IReadOnlyList<string> AllowedBases { get; }

    /// <summary>Exact folders outside those bases that are allowed, such as the driver extraction folders.</summary>
    IReadOnlyList<string> AllowedExact { get; }

    IReadOnlyList<string> SteamLibraries();
    IReadOnlyList<string> WowVersionFolders();
    IReadOnlySet<string> RunningProcesses();
    bool QueueDeleteAtRestart(string file);

    /// <summary>True when another program has the file open so that it cannot be deleted right now.</summary>
    bool IsLocked(string file);

    /// <summary>Clarion's own data folder, where event log copies are kept.</summary>
    string DataFolder { get; }
    long RecycleBinBytes();
    void EmptyRecycleBin();
    IReadOnlyList<string> EventLogNames();
    /// <summary>Saves the log to the backup file, then clears it. Must not clear the log if the copy cannot be saved.</summary>
    void ClearEventLog(string name, string backupFile);
}
