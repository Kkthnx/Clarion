using Clarion.Engine;

namespace Clarion.App.Services;

/// <summary>A small log file kept on this PC only. It is trimmed when it grows past 1 MB.</summary>
public static class Log
{
    private static readonly object Gate = new();
    private const long MaxBytes = 1_000_000;

    public static string Path { get; } = System.IO.Path.Combine(EngineFactory.DefaultDataDirectory, "clarion.log");

    /// <summary>The last lines of the log, or none when there is no log yet.</summary>
    public static IReadOnlyList<string> Tail(int lines)
    {
        try
        {
            lock (Gate)
            {
                if (!File.Exists(Path)) return [];
                using var stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                var all = new List<string>();
                while (reader.ReadLine() is { } line) all.Add(line);
                return all.TakeLast(lines).ToList();
            }
        }
        catch (IOException) { return []; }
        catch (UnauthorizedAccessException) { return []; }
    }

    public static void Write(string message)
    {
        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(EngineFactory.DefaultDataDirectory);
                if (File.Exists(Path) && new FileInfo(Path).Length > MaxBytes)
                {
                    File.Move(Path, Path + ".old", overwrite: true);
                }
                File.AppendAllText(Path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
