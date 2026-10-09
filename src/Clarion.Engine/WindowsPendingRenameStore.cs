using System.Runtime.Versioning;
using Clarion.Core.Cleanup;
using Microsoft.Win32;

namespace Clarion.Engine;

/// <summary>The Windows list of file operations to run at the next restart, kept in a registry value.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsPendingRenameStore : IPendingRenameStore
{
    private const string Key = "SYSTEM\\CurrentControlSet\\Control\\Session Manager";
    private const string Value = "PendingFileRenameOperations";

    public IReadOnlyList<string> Read()
    {
        using var key = Registry.LocalMachine.OpenSubKey(Key);
        return key?.GetValue(Value) as string[] ?? [];
    }

    public void Write(IReadOnlyList<string> entries)
    {
        using var key = Registry.LocalMachine.OpenSubKey(Key, writable: true)
                        ?? throw new InvalidOperationException("The Windows restart list could not be opened for writing.");
        if (entries.Count == 0) key.DeleteValue(Value, throwOnMissingValue: false);
        else key.SetValue(Value, entries.ToArray(), RegistryValueKind.MultiString);
    }
}
