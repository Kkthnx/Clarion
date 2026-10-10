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

    /// <summary>
    /// PowerShell writes its errors as an XML document, with a first line of "#&lt; CLIXML", when its error output is redirected, and
    /// that first line is all a person would have seen. This turns the document back into the plain error text.
    /// </summary>
    public static string DecodeErrorStream(string error)
    {
        if (!error.StartsWith("#< CLIXML", StringComparison.Ordinal)) return error;
        var lines = new List<string>();
        foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(error, "<S S=\"Error\">(.*?)</S>", System.Text.RegularExpressions.RegexOptions.Singleline))
        {
            var text = System.Net.WebUtility.HtmlDecode(m.Groups[1].Value)
                .Replace("_x000D__x000A_", "\n", StringComparison.Ordinal).Replace("_x000A_", "\n", StringComparison.Ordinal).Replace("_x000D_", "", StringComparison.Ordinal);
            lines.Add(text);
        }
        var joined = string.Join("\n", lines).Trim();
        return joined.Length > 0 ? joined : error;
    }

    /// <summary>The first non-empty line of the error text, or of the output when there is no error text.</summary>
    public static string FirstLine(string error, string output, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(error) ? output : DecodeErrorStream(error);
        return text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? fallback;
    }
}
