using Clarion.Core.Abstractions;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class FakeRegistry : IRegistryStore
{
    private readonly Dictionary<string, RegistryData> _values = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _keys = new(StringComparer.OrdinalIgnoreCase);

    public Func<RegistryTarget, bool>? FailWrite { get; set; }

    private static string KeyId(RegistryTarget t) => $"{t.Hive}\\{t.Path}";
    private static string ValueId(RegistryTarget t) => $"{KeyId(t)}\\{t.Name}";

    public RegistrySnapshot Read(RegistryTarget target)
    {
        var segments = target.Path.Split('\\');
        var deepest = new List<string>();
        foreach (var seg in segments)
        {
            var next = $"{target.Hive}\\{string.Join('\\', deepest.Append(seg))}";
            if (!_keys.Contains(next)) break;
            deepest.Add(seg);
        }
        var deepestPath = string.Join('\\', deepest);
        return _values.TryGetValue(ValueId(target), out var data)
            ? new RegistrySnapshot(true, data, deepestPath)
            : new RegistrySnapshot(false, null, deepestPath);
    }

    public void Write(RegistryTarget target, RegistryData data)
    {
        if (FailWrite?.Invoke(target) == true) throw new UnauthorizedAccessException("denied");
        var parts = target.Path.Split('\\');
        for (var i = 1; i <= parts.Length; i++) _keys.Add($"{target.Hive}\\{string.Join('\\', parts.Take(i))}");
        _values[ValueId(target)] = data;
    }

    public void DeleteValue(RegistryTarget target) => _values.Remove(ValueId(target));

    public void PruneEmptyKeys(RegistryTarget target, string keepAncestor)
    {
        var parts = target.Path.Split('\\');
        var keep = keepAncestor.Length == 0 ? 0 : keepAncestor.Split('\\').Length;
        for (var depth = parts.Length; depth > keep; depth--)
        {
            var id = $"{target.Hive}\\{string.Join('\\', parts.Take(depth))}";
            var occupied = _values.Keys.Any(k => k.StartsWith(id + "\\", StringComparison.OrdinalIgnoreCase))
                           || _keys.Any(k => k.StartsWith(id + "\\", StringComparison.OrdinalIgnoreCase));
            if (occupied) return;
            _keys.Remove(id);
        }
    }

    public bool HasKey(RegistryHive hive, string path) => _keys.Contains($"{hive}\\{path}");
}
