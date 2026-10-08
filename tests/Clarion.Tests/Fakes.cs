using Clarion.Core.Abstractions;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class FakeServices : IServiceStore
{
    private readonly Dictionary<string, ServiceStartType> _map = new(StringComparer.OrdinalIgnoreCase);
    public void Set(string name, ServiceStartType type) => _map[name] = type;
    public ServiceStartType? GetStartType(string name) => _map.TryGetValue(name, out var t) ? t : null;
    public void SetStartType(string name, ServiceStartType startType) => _map[name] = startType;
}

public sealed class FakeTasks : ITaskStore
{
    private readonly Dictionary<string, bool> _map = new(StringComparer.OrdinalIgnoreCase);
    public void Set(string path, bool enabled) => _map[path] = enabled;
    public bool? GetEnabled(string path) => _map.TryGetValue(path, out var e) ? e : null;
    public void SetEnabled(string path, bool enabled) => _map[path] = enabled;
}

public sealed class FakeProcessRunner : IProcessRunner
{
    public List<string> Calls { get; } = [];
    public Func<string, ProcessResult> Respond { get; set; } = _ => new ProcessResult(0, "", "");

    public ProcessResult Run(string fileName, string arguments, TimeSpan timeout)
    {
        Calls.Add($"{fileName} {arguments}");
        return Respond(arguments);
    }
}
