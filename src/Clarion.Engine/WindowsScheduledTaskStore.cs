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
        // The file is written where only an administrator can change it. In the account's own temp folder, a program running as the same
        // account without administrator rights could swap the file between this write and schtasks reading it, and so choose what
        // the elevated task runs. Users can add files to the Windows temp folder but cannot change one that an administrator made.
        var windowsTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
        var folder = Directory.Exists(windowsTemp) ? windowsTemp : Path.GetTempPath();
        var file = Path.Combine(folder, "clarion-task-" + Guid.NewGuid().ToString("N") + ".xml");
        try
        {
            using (var stream = new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UnicodeEncoding(false, true)))
                writer.Write(xml);
            var result = runner.Run("schtasks.exe", $"/Create /TN \"{Safe(name)}\" /XML \"{file}\" /F", Timeout);
            if (result.ExitCode != 0)
                throw new InvalidOperationException(Clarion.Core.Engine.PowerShellHost.FirstLine(result.Error, result.Output, "Task Scheduler refused the task."));
        }
        finally
        {
            try { File.Delete(file); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    public string? ReadXml(string name)
    {
        var result = runner.Run("schtasks.exe", $"/Query /TN \"{Safe(name)}\" /XML", Timeout);
        return result.ExitCode == 0 ? result.Output : null;
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
