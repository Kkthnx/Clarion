using System.Text.RegularExpressions;

namespace Clarion.Core.Feedback;

/// <summary>
/// Removes what identifies a person or a PC from text that will be shared: the Windows user name,
/// the computer name, user folders, e-mail addresses, network addresses and account identifiers.
/// </summary>
public static partial class Sanitizer
{
    public static string Clean(string text, string userName, string machineName)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var t = Sid().Replace(text, "<account id>");
        t = Email().Replace(t, "<email>");
        t = Mac().Replace(t, "<mac address>");
        t = Ipv6().Replace(t, "<ip address>");
        t = Ipv4().Replace(t, "<ip address>");
        t = UserFolder().Replace(t, "$1<user>");
        t = Replace(t, userName, "<user>");
        t = Replace(t, machineName, "<computer>");
        return t;
    }

    // Short names such as "a" would wreck the text, so only names that are long enough to be meaningful are replaced.
    private static string Replace(string text, string value, string with) =>
        value.Trim().Length >= 3 ? Regex.Replace(text, Regex.Escape(value.Trim()), with, RegexOptions.IgnoreCase) : text;

    [GeneratedRegex(@"S-1-\d+(-\d+){2,}", RegexOptions.IgnoreCase)]
    private static partial Regex Sid();

    [GeneratedRegex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}")]
    private static partial Regex Email();

    [GeneratedRegex(@"\b[0-9A-Fa-f]{2}([:\-][0-9A-Fa-f]{2}){5}\b")]
    private static partial Regex Mac();

    [GeneratedRegex(@"\b(?:[0-9A-Fa-f]{1,4}:){4,7}[0-9A-Fa-f]{1,4}\b")]
    private static partial Regex Ipv6();

    [GeneratedRegex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b")]
    private static partial Regex Ipv4();

    [GeneratedRegex(@"([A-Za-z]:\\Users\\)[^\\/\s""']+", RegexOptions.IgnoreCase)]
    private static partial Regex UserFolder();
}
