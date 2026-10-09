using System.Runtime.Versioning;
using Clarion.Core.Engine;
using Clarion.Core.Journal;

namespace Clarion.Engine;

/// <summary>Wires the real Windows stores into the engine and the batch runner.</summary>
[SupportedOSPlatform("windows")]
public static class EngineFactory
{
    public static string DefaultDataDirectory { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Clarion");

    public static BatchRunner CreateBatchRunner(string? dataDirectory = null) => Create(dataDirectory).Runner;

    public static ClarionRuntime Create(string? dataDirectory = null)
    {
        var dir = dataDirectory ?? DefaultDataDirectory;
        var registry = new WindowsRegistryStore();
        var runner = new WindowsProcessRunner();
        var handlers = new Core.Abstractions.IOperationHandler[]
        {
            new RegistryHandler(registry),
            new ServiceHandler(new WindowsServiceStore(runner)),
            new TaskHandler(new WindowsTaskStore()),
            new AppxHandler(new WindowsAppxStore(runner)),
            new FeatureHandler(new WindowsFeatureStore(runner)),
            new PowerHandler(new WindowsPowerStore(runner)),
            new DnsHandler(new WindowsDnsStore(runner)),
        };
        var journal = new ChangeJournal(Path.Combine(dir, "journal.jsonl"));
        var engine = new TweakEngine(handlers, journal);
        var drift = new Core.Drift.DriftScanner(engine, journal, new Core.Drift.DriftState(Path.Combine(dir, "drift.json")));
        var batch = new BatchRunner(engine, new RestorePointService(registry, runner), WindowsMachine.IsElevated);
        var restorePoints = new RestorePointService(registry, runner);
        return new ClarionRuntime(batch, engine, journal, WindowsMachine.Detect(), restorePoints, new Core.Actions.ActionRunner(new WindowsStreamingRunner()),
            new Core.SystemInfo.SystemProbe(registry, new WindowsServiceStore(runner), runner), drift);
    }
}

public sealed record ClarionRuntime(
    BatchRunner Runner, TweakEngine Engine, ChangeJournal Journal, Clarion.Core.Model.MachineProfile Profile,
    IRestorePointService RestorePoints, Core.Actions.ActionRunner Actions, Core.SystemInfo.SystemProbe System, Core.Drift.DriftScanner Drift);