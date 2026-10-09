using System.Runtime.Versioning;
using System.Text;
using Clarion.Core.Abstractions;
using Clarion.Core.Scheduling;

namespace Clarion.Engine;

/// <summary>Talks to Task Scheduler through schtasks.exe, which ships with Windows. Needs administrator rights to create or delete.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsScheduledTaskStore(IProcessRunner runner) : IScheduledTaskStore
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30);

    public bool Exists(string name) => runner.Run("schtasks.exe", $"/Query /TN \"{Safe(name)}\"", Timeout).ExitCode == 0;

    public void Create(string name, string xml)
    {
        // schtasks reads task files as UTF-16, which is what the XML declares.
        var file = Path.Combine(Path.GetTempPath(), "clarion-task-" + Guid.NewGuid().ToString("N") + ".xml");
        try
        {
            File.WriteAllText(file, xml, new UnicodeEncoding(false, true));
            var result = runner.Run("schtasks.exe", $"/Create /TN \"{Safe(name)}\" /XML \"{file}\" /F", Timeout);
            if (result.ExitCode != 0)
                throw new InvalidOperationException(Clarion.Core.Engine.PowerShellHost.FirstLine(result.Error, result.Output, "Task Scheduler refused the task."));
        }
        finally
        {
            try { File.Delete(file); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    public void Delete(string name)
    {
        var result = runner.Run("schtasks.exe", $"/Delete /TN \"{Safe(name)}\" /F", Timeout);
        if (result.ExitCode != 0)
            throw new InvalidOperationException(Clarion.Core.Engine.PowerShellHost.FirstLine(result.Error, result.Output, "Task Scheduler could not remove the task."));
    }

    // A task name is only ever Clarion's own constant. This keeps a quote from ever reaching the command line.
    private static string Safe(string name) =>
        name.Contains('"') ? throw new ArgumentException("Task names cannot contain a quote.", nameof(name)) : name;
}
