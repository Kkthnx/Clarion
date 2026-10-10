using System.Security;
using System.Text;
using System.Text.Json;
using Clarion.Core.Drift;

namespace Clarion.Core.Scheduling;

/// <summary>Creates, finds and removes the one Windows scheduled task Clarion can own.</summary>
public interface IScheduledTaskStore
{
    bool Exists(string name);

    /// <summary>Creates the task from its XML, replacing one with the same name.</summary>
    void Create(string name, string xml);

    void Delete(string name);

    /// <summary>The task's definition as XML, or null when there is no such task.</summary>
    string? ReadXml(string name);
}

/// <summary>
/// The monthly check: a scheduled task that runs the read only <c>--verify --save</c> on the second Wednesday of the month, the day after
/// Windows' monthly patch day, which is when changes are most likely to have been undone. It runs as the person who turned it on, so their own
/// settings are the ones checked, and if the PC was off it runs at the next chance. It changes nothing and sends nothing.
/// </summary>
public sealed class MonthlyVerifySchedule(IScheduledTaskStore store)
{
    public const string TaskName = "\\Clarion\\Monthly check";
    public const string Arguments = "--verify --save";

    public bool IsEnabled() => store.Exists(TaskName);

    /// <summary>The program the task starts, or null when there is no task or its definition cannot be read.</summary>
    public string? CurrentCommand()
    {
        var xml = store.ReadXml(TaskName);
        return xml is null ? null : ParseCommand(xml);
    }

    /// <summary>
    /// True when the task exists and starts a different program file than this one, which is what happens after Clarion is moved to
    /// another folder. A path with letters outside plain ASCII is not compared, because the text Task Scheduler prints can be changed
    /// by the console's code page and a false alarm would be worse than no check.
    /// </summary>
    public bool PointsElsewhere(string thisExe)
    {
        var command = CurrentCommand();
        if (command is null) return false;
        if (command.Any(c => c > 127) || thisExe.Any(c => c > 127)) return false;
        return !string.Equals(command.Trim().Trim('"'), thisExe.Trim().Trim('"'), StringComparison.OrdinalIgnoreCase);
    }

    public static string? ParseCommand(string xml)
    {
        var m = System.Text.RegularExpressions.Regex.Match(xml, "<Command>(.*?)</Command>", System.Text.RegularExpressions.RegexOptions.Singleline);
        return m.Success ? System.Net.WebUtility.HtmlDecode(m.Groups[1].Value) : null;
    }

    public void Enable(string exePath, string userId, DateTime firstDay) => store.Create(TaskName, BuildXml(exePath, userId, firstDay));

    public void Disable()
    {
        if (store.Exists(TaskName)) store.Delete(TaskName);
    }

    /// <summary>The task definition in the format Task Scheduler imports. Text from outside is escaped, not trusted.</summary>
    public static string BuildXml(string exePath, string userId, DateTime firstDay)
    {
        string E(string text) => SecurityElement.Escape(text) ?? "";
        var start = firstDay.Date.AddHours(9).ToString("yyyy-MM-dd'T'HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
        var months = string.Join("", new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }.Select(m => $"<{m} />"));

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-16\"?>");
        sb.AppendLine("<Task version=\"1.4\" xmlns=\"http://schemas.microsoft.com/windows/2004/02/mit/task\">");
        sb.AppendLine("  <RegistrationInfo>");
        sb.AppendLine("    <Description>Checks that the settings Clarion applied are still in place, the day after Windows' monthly updates. Read only. Nothing is sent anywhere.</Description>");
        sb.AppendLine($"    <URI>{E(TaskName)}</URI>");
        sb.AppendLine("  </RegistrationInfo>");
        sb.AppendLine("  <Triggers>");
        sb.AppendLine("    <CalendarTrigger>");
        sb.AppendLine($"      <StartBoundary>{start}</StartBoundary>");
        sb.AppendLine("      <Enabled>true</Enabled>");
        sb.AppendLine("      <ScheduleByMonthDayOfWeek>");
        sb.AppendLine("        <Weeks><Week>2</Week></Weeks>");
        sb.AppendLine("        <DaysOfWeek><Wednesday /></DaysOfWeek>");
        sb.AppendLine($"        <Months>{months}</Months>");
        sb.AppendLine("      </ScheduleByMonthDayOfWeek>");
        sb.AppendLine("    </CalendarTrigger>");
        sb.AppendLine("  </Triggers>");
        sb.AppendLine("  <Principals>");
        sb.AppendLine("    <Principal id=\"Author\">");
        sb.AppendLine($"      <UserId>{E(userId)}</UserId>");
        sb.AppendLine("      <LogonType>InteractiveToken</LogonType>");
        sb.AppendLine("      <RunLevel>HighestAvailable</RunLevel>");
        sb.AppendLine("    </Principal>");
        sb.AppendLine("  </Principals>");
        sb.AppendLine("  <Settings>");
        sb.AppendLine("    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>");
        sb.AppendLine("    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>");
        sb.AppendLine("    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>");
        sb.AppendLine("    <StartWhenAvailable>true</StartWhenAvailable>");
        sb.AppendLine("    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>");
        sb.AppendLine("    <AllowStartOnDemand>true</AllowStartOnDemand>");
        sb.AppendLine("    <Enabled>true</Enabled>");
        sb.AppendLine("    <ExecutionTimeLimit>PT15M</ExecutionTimeLimit>");
        sb.AppendLine("    <Priority>7</Priority>");
        sb.AppendLine("  </Settings>");
        sb.AppendLine("  <Actions Context=\"Author\">");
        sb.AppendLine("    <Exec>");
        sb.AppendLine($"      <Command>{E(exePath)}</Command>");
        sb.AppendLine($"      <Arguments>{Arguments}</Arguments>");
        sb.AppendLine("    </Exec>");
        sb.AppendLine("  </Actions>");
        sb.AppendLine("</Task>");
        return sb.ToString();
    }
}

/// <summary>What the last scheduled check found, kept for the next time Clarion is opened.</summary>
public sealed record ScheduledVerifyResult(
    DateTimeOffset At, int Build, int Checked,
    IReadOnlyList<string> ChangedBack, IReadOnlyList<string> ReturnedApps, int Unreadable, bool Seen = false)
{
    public int Attention => ChangedBack.Count + ReturnedApps.Count;

    public static ScheduledVerifyResult From(DriftReport report, DateTimeOffset at) => new(
        at, report.Build, report.Checked,
        report.ChangedBack.Select(i => i.Tweak.Id).ToList(),
        report.ReturnedApps.Select(i => i.Tweak.Id).ToList(),
        report.Unreadable.Count);
}

public sealed class ScheduledVerifyStore(string path)
{
    private readonly object _gate = new();

    public void Save(ScheduledVerifyResult result)
    {
        lock (_gate)
        {
            try
            {
                var dir = Path.GetDirectoryName(Path.GetFullPath(path));
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                var temp = path + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(result));
                File.Move(temp, path, overwrite: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { }
        }
    }

    /// <summary>A missing or damaged file means there is no result. It is never an error.</summary>
    public ScheduledVerifyResult? Load()
    {
        lock (_gate)
        {
            try { return File.Exists(path) ? JsonSerializer.Deserialize<ScheduledVerifyResult>(File.ReadAllText(path)) : null; }
            catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException) { return null; }
        }
    }

    public void MarkSeen()
    {
        if (Load() is { Seen: false } r) Save(r with { Seen = true });
    }
}
