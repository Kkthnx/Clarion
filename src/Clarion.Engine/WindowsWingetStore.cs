using System.Runtime.Versioning;
using Clarion.Core.Abstractions;
using Clarion.Core.Apps;

namespace Clarion.Engine;

/// <summary>
/// Runs winget. It is found at its fixed place for this account, the app execution alias folder, and never by searching the path, because
/// Clarion runs with administrator rights and a program of the same name beside it or in the current folder would run with them.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsWingetStore(IProcessRunner runner, string? wingetPath = null) : IWingetStore
{
    private static readonly TimeSpan QueryTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan InstallTimeout = TimeSpan.FromMinutes(30);

    private readonly string _path = wingetPath ?? DefaultPath;
    private readonly object _gate = new();
    private IReadOnlySet<string>? _installed;
    private string? _failure;

    public static string DefaultPath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "WindowsApps", "winget.exe");

    public bool IsAvailable => File.Exists(_path);

    public bool AcceptPackageAgreements { get; set; }

    public void Invalidate()
    {
        lock (_gate) { _installed = null; _failure = null; }
    }

    /// <summary>
    /// One export call lists every installed package winget can match to its community source, as JSON, which does not change with the
    /// language of Windows. A failed read is remembered until the next refresh, so each app does not run winget again to fail the same way.
    /// </summary>
    public IReadOnlySet<string> InstalledIds()
    {
        lock (_gate)
        {
            if (_installed is not null) return _installed;
            if (_failure is not null) throw new InvalidOperationException(_failure);

            var file = Path.Combine(Path.GetTempPath(), $"clarion-winget-{Guid.NewGuid():N}.json");
            try
            {
                var result = runner.Run(_path, WingetCommands.Export(file), QueryTimeout);
                // winget can exit with a warning code about packages it cannot match and still write the file, so the file decides.
                if (!File.Exists(file)) throw new InvalidOperationException(WingetCodes.Explain(result.ExitCode));
                return _installed = WingetExport.InstalledIds(File.ReadAllText(file));
            }
            catch (Exception ex) when (ex is InvalidOperationException or IOException or System.Text.Json.JsonException or System.ComponentModel.Win32Exception)
            {
                _failure = ex.Message;
                throw new InvalidOperationException(ex.Message, ex);
            }
            finally
            {
                try { File.Delete(file); } catch (IOException) { }
            }
        }
    }

    public WingetOutcome Install(string id) => Run(WingetCommands.Install(id), InstallTimeout);

    public WingetOutcome Uninstall(string id) => Run(WingetCommands.Uninstall(id), InstallTimeout);

    public WingetPackageInfo? Show(string id)
    {
        try
        {
            var result = runner.Run(_path, WingetCommands.Show(id), QueryTimeout);
            return result.ExitCode == 0 ? WingetShow.Parse(result.Output) : null;
        }
        catch (System.ComponentModel.Win32Exception) { return null; }
        catch (InvalidOperationException) { return null; }
    }

    private WingetOutcome Run(string arguments, TimeSpan timeout)
    {
        var result = runner.Run(_path, arguments, timeout);
        return new WingetOutcome(result.ExitCode, string.IsNullOrWhiteSpace(result.Error) ? result.Output : result.Output + Environment.NewLine + result.Error);
    }
}
