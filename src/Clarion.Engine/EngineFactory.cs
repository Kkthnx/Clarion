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

    public static BatchRunner CreateBatchRunner(string? dataDirectory = null)
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
        };
        var engine = new TweakEngine(handlers, new ChangeJournal(Path.Combine(dir, "journal.jsonl")));
        return new BatchRunner(engine, new RestorePointService(registry, runner), WindowsMachine.IsElevated);
    }
}
