using System.Runtime.Versioning;
using Clarion.Core.Abstractions;
using Clarion.Core.Model;
using Microsoft.Win32;
using CoreHive = Clarion.Core.Model.RegistryHive;
using CoreKind = Clarion.Core.Model.RegistryKind;

namespace Clarion.Engine;

[SupportedOSPlatform("windows")]
public sealed class WindowsRegistryStore : IRegistryStore
{
    public RegistrySnapshot Read(RegistryTarget target)
    {
        using var root = OpenRoot(target.Hive);
        var segments = Split(target.Path);

        var deepest = new List<string>();
        RegistryKey? current = root;
        var owned = new List<RegistryKey>();
        try
        {
            foreach (var seg in segments)
            {
                var next = current?.OpenSubKey(seg, writable: false);
                if (next is null) { current = null; break; }
                owned.Add(next);
                current = next;
                deepest.Add(seg);
            }

            var deepestPath = string.Join('\\', deepest);
            if (current is null || deepest.Count != segments.Length)
                return new RegistrySnapshot(false, null, deepestPath);

            var value = current.GetValue(target.Name, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
            if (value is null) return new RegistrySnapshot(false, null, deepestPath);

            return new RegistrySnapshot(true, ToData(value, current.GetValueKind(target.Name)), deepestPath);
        }
        finally
        {
            foreach (var k in owned) k.Dispose();
        }
    }

    public void Write(RegistryTarget target, RegistryData data)
    {
        using var root = OpenRoot(target.Hive);
        using var key = root.CreateSubKey(target.Path, writable: true)
                        ?? throw new IOException($"Could not open {target.Path}");
        var (value, kind) = FromData(data);
        key.SetValue(target.Name, value, kind);
    }

    public void DeleteValue(RegistryTarget target)
    {
        using var root = OpenRoot(target.Hive);
        using var key = root.OpenSubKey(target.Path, writable: true);
        key?.DeleteValue(target.Name, throwOnMissingValue: false);
    }

    public void PruneEmptyKeys(RegistryTarget target, string keepAncestor)
    {
        using var root = OpenRoot(target.Hive);
        var segments = Split(target.Path);
        var keep = Split(keepAncestor).Length;

        for (var depth = segments.Length; depth > keep; depth--)
        {
            using (var key = root.OpenSubKey(string.Join('\\', segments.Take(depth))))
            {
                if (key is null) continue;
                if (key.ValueCount != 0 || key.SubKeyCount != 0) return;
            }
            using var parent = depth == 1 ? null : root.OpenSubKey(string.Join('\\', segments.Take(depth - 1)), writable: true);
            if (depth == 1) root.DeleteSubKey(segments[0], throwOnMissingSubKey: false);
            else parent?.DeleteSubKey(segments[depth - 1], throwOnMissingSubKey: false);
        }
    }

    private static RegistryKey OpenRoot(CoreHive hive) =>
        RegistryKey.OpenBaseKey(hive == CoreHive.CurrentUser ? Microsoft.Win32.RegistryHive.CurrentUser : Microsoft.Win32.RegistryHive.LocalMachine,
            RegistryView.Registry64);

    private static string[] Split(string path) =>
        path.Split('\\', StringSplitOptions.RemoveEmptyEntries);

    private static RegistryData ToData(object value, RegistryValueKind kind) => kind switch
    {
        RegistryValueKind.DWord => new RegistryData(CoreKind.DWord, unchecked((uint)(int)value).ToString()),
        RegistryValueKind.QWord => new RegistryData(CoreKind.QWord, unchecked((ulong)(long)value).ToString()),
        RegistryValueKind.ExpandString => new RegistryData(CoreKind.ExpandString, (string)value),
        RegistryValueKind.String => new RegistryData(CoreKind.String, (string)value),
        _ => new RegistryData(CoreKind.Binary, Convert.ToHexString(value as byte[] ?? [])),
    };

    private static (object Value, RegistryValueKind Kind) FromData(RegistryData data) => data.Kind switch
    {
        CoreKind.DWord => (unchecked((int)uint.Parse(data.Value)), RegistryValueKind.DWord),
        CoreKind.QWord => (unchecked((long)ulong.Parse(data.Value)), RegistryValueKind.QWord),
        CoreKind.ExpandString => (data.Value, RegistryValueKind.ExpandString),
        CoreKind.Binary => (Convert.FromHexString(data.Value), RegistryValueKind.Binary),
        _ => (data.Value, RegistryValueKind.String),
    };
}
