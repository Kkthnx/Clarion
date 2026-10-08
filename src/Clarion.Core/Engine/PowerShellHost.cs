using System.Text;
using Clarion.Core.Abstractions;

namespace Clarion.Core.Engine;

/// <summary>
/// The one place that turns a PowerShell script into a safe command line and runs it. Scripts are passed as
/// encoded commands, so nothing in them can be changed by quoting. Errors come back as the first useful line.
/// </summary>
public static class PowerShellHost
{
    public const string Prelude = "$ErrorActionPreference='Stop'; [Console]::OutputEncoding=[Text.Encoding]::UTF8; ";

    public static string ToArguments(string script) =>
        "-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand " +
        Convert.ToBase64String(Encoding.Unicode.GetBytes(Prelude + script));

    /// <summary>The script inside an encoded command line, for tests and logs.</summary>
    public static string Decode(string arguments)
    {
        const string marker = "-EncodedCommand ";
        var at = arguments.IndexOf(marker, StringComparison.Ordinal);
        return at < 0 ? arguments : Encoding.Unicode.GetString(Convert.FromBase64String(arguments[(at + marker.Length)..]));
    }

    /// <summary>Runs the script and returns its output. Throws with the first line of the error when it fails.</summary>
    public static string Run(IProcessRunner runner, string script, TimeSpan timeout)
    {
        var result = runner.Run("powershell.exe", ToArguments(script), timeout);
        if (result.ExitCode != 0) throw new InvalidOperationException(FirstLine(result.Error, result.Output, "PowerShell command failed."));
        return result.Output;
    }

    /// <summary>The first non-empty line of the error text, or of the output when there is no error text.</summary>
    public static string FirstLine(string error, string output, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(error) ? output : error;
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? fallback;
    }
}
