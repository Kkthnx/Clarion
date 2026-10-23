using System.Runtime.Versioning;
using Clarion.Core.Engine;
using Clarion.Core.Journal;

namespace Clarion.Engine;

/// <summary>Wires the real Windows stores into the engine and the batch runner.</summary>
[SupportedOSPlatform("windows")]
public static class EngineFactory
{
    /// <summary>
    /// Where the history, settings and log are kept: the Clarion folder under ProgramData, unless CLARION_DATA_DIR names another folder.
    /// The override is for trying Clarion with a clean slate, and for a portable copy. It is not a setting people need.
    /// </summary>
    public static string DefaultDataDirectory { get; } =
        Environment.GetEnvironmentVariable("CLARION_DATA_DIR") is { Length: > 0 } custom
            ? Path.GetFullPath(custom)
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Clarion");

    /// <summary>True when CLARION_DATA_DIR points somewhere else, which is a folder somebody chose and is left as it is.</summary>
    public static bool IsDataDirectoryOverridden => Environment.GetEnvironmentVariable("CLARION_DATA_DIR") is { Length: > 0 };

    /// <summary>What locking the data folder had to correct on this start, in plain words. Empty when it was already right.</summary>
    public static IReadOnlyList<string> DataFolderNotes { get; private set; } = [];

    private static int _folderChecked;

    public static BatchRunner CreateBatchRunner(string? dataDirectory = null) => Create(dataDirectory).Runner;

    public static ClarionRuntime Create(string? dataDirectory = null)
    {
        var dir = dataDirectory ?? DefaultDataDirectory;
        // The shared folder holds the journal that a Revert replays with administrator rights, so with those rights it is locked first.
        if (dataDirectory is null && !IsDataDirectoryOverridden && WindowsMachine.IsElevated() && Interlocked.Exchange(ref _folderChecked, 1) == 0)
            DataFolderNotes = DataFolderGuard.Secure(dir);
        var registry = new WindowsRegistryStore();
        var runner = new WindowsProcessRunner();
        var winget = new WindowsWingetStore(runner);
        var handlers = new Core.Abstractions.IOperationHandler[]
        {
            new RegistryHandler(registry),
            new ServiceHandler(new WindowsServiceStore(runner)),
            new TaskHandler(new WindowsTaskStore()),
            new AppxHandler(new WindowsAppxStore(runner)),
            new FeatureHandler(new WindowsFeatureStore(runner)),
            new PowerHandler(new WindowsPowerStore(runner)),
            new DnsHandler(new WindowsDnsStore(runner)),
            new WingetHandler(winget),
            new RegistryListHandler(registry),
            new StartupHandler(registry),
            new NvidiaHandler(new NvapiDriverStore()),
        };
        var journal = new ChangeJournal(Path.Combine(dir, "journal.jsonl"), System.Security.Principal.WindowsIdentity.GetCurrent().User?.Value);
        var engine = new TweakEngine(handlers, journal);
        var drift = new Core.Drift.DriftScanner(engine, journal, new Core.Drift.DriftState(Path.Combine(dir, "drift.json")), history: new Core.Drift.DriftHistory(Path.Combine(dir, "drift-history.jsonl")));
        var batch = new BatchRunner(engine, new RestorePointService(registry, runner), WindowsMachine.IsElevated);
        var restorePoints = new RestorePointService(registry, runner);
        return new ClarionRuntime(batch, engine, journal, WindowsMachine.Detect(), restorePoints, new Core.Actions.ActionRunner(new WindowsStreamingRunner(), null, new WindowsDisplayAdapters()),
            new Core.SystemInfo.SystemProbe(registry, new WindowsServiceStore(runner), runner, new WindowsTaskStore()), drift, winget, new WindowsServiceLister());
    }
}

public sealed record ClarionRuntime(
    BatchRunner Runner, TweakEngine Engine, ChangeJournal Journal, Clarion.Core.Model.MachineProfile Profile,
    IRestorePointService RestorePoints, Core.Actions.ActionRunner Actions, Core.SystemInfo.SystemProbe System, Core.Drift.DriftScanner Drift,
    Clarion.Core.Apps.IWingetStore Winget, Clarion.Core.WindowsServices.IServiceLister Services);
