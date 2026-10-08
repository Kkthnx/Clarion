using Clarion.Core.Abstractions;
using Clarion.Core.Model;

namespace Clarion.Core.Engine;

public interface IRestorePointService
{
    TweakResult Create(string description);
    TweakResult EnableProtection(string drive);
}

/// <summary>
/// Creates a System Restore point through the supported cmdlet. Windows limits automatic
/// creation to one per 24 hours, so the frequency value is set to 0 for the call and then
/// put back exactly as it was.
/// </summary>
public sealed class RestorePointService(IRegistryStore registry, IProcessRunner runner) : IRestorePointService
{
    private static readonly RegistryTarget Frequency = new(
        RegistryHive.LocalMachine,
        "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore",
        "SystemRestorePointCreationFrequency");

    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

    public TweakResult Create(string description)
    {
        var safe = Sanitize(description);
        var prior = registry.Read(Frequency);
        try
        {
            registry.Write(Frequency, new RegistryData(RegistryKind.DWord, "0"));
            var result = runner.Run("powershell.exe", PowerShellHost.ToArguments($"Checkpoint-Computer -Description '{safe}' -RestorePointType MODIFY_SETTINGS -ErrorAction Stop"), Timeout);
            return result.ExitCode == 0
                ? TweakResult.Ok()
                : TweakResult.Fail(PowerShellHost.FirstLine(result.Error, result.Output, "Restore point failed."));
        }
        finally
        {
            if (prior.Exists && prior.Data is not null) registry.Write(Frequency, prior.Data);
            else registry.DeleteValue(Frequency);
        }
    }

    public TweakResult EnableProtection(string drive)
    {
        var safe = Sanitize(drive);
        var result = runner.Run("powershell.exe", PowerShellHost.ToArguments($"Enable-ComputerRestore -Drive '{safe}' -ErrorAction Stop"), Timeout);
        return result.ExitCode == 0
            ? TweakResult.Ok()
            : TweakResult.Fail(PowerShellHost.FirstLine(result.Error, result.Output, "Could not turn on System Protection."));
    }

    private static string Sanitize(string text) =>
        new(text.Where(c => char.IsLetterOrDigit(c) || c is ' ' or ':' or '\\' or '-' or '_' or '.').ToArray());
}
