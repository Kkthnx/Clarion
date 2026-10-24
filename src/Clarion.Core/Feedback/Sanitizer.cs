using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Clarion.Core.Feedback;

/// <summary>
/// Removes what identifies a person or a PC from text that will be shared: the Windows user name,
/// the computer name, user folders, e-mail addresses, network addresses and account identifiers.
/// </summary>
public static partial class Sanitizer
{
    // Names every Windows PC has. They say nothing about a person, and replacing them would turn "Running as administrator" into
    // "Running as <user>" and the Administrators group into "<user>s".
    private static readonly HashSet<string> GenericAccountNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "administrator", "admin", "user", "owner", "guest", "default", "public", "system",
    };

    public static string Clean(string text, string userName, string machineName)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var t = Sid().Replace(text, "<account id>");
        t = Email().Replace(t, "<email>");
        t = Mac().Replace(t, "<mac address>");
        t = Ipv6Candidate().Replace(t, m => IsIpv6(m.Value) ? "<ip address>" : m.Value);
        // Four numbers with dots are an address only when each is at most 255. A version such as 1.2.94.576 is not, and stays readable.
        t = Ipv4().Replace(t, m => IPAddress.TryParse(m.Value, out var ip) && ip.AddressFamily == AddressFamily.InterNetwork ? "<ip address>" : m.Value);
        t = UserFolder().Replace(t, "$1<user>");
        t = UncServer().Replace(t, @"\\<server>");
        if (!GenericAccountNames.Contains(userName.Trim())) t = Replace(t, userName, "<user>");
        t = Replace(t, machineName, "<computer>");
        return t;
    }

    // Short names such as "a" would wreck the text, so only names that are long enough to be meaningful are replaced. Only the whole
    // name is replaced, so a name that is part of an ordinary word does not tear that word apart.
    private static string Replace(string text, string value, string with)
    {
        var name = value.Trim();
        return name.Length >= 3
            ? Regex.Replace(text, $"(?<![A-Za-z0-9]){Regex.Escape(name)}(?![A-Za-z0-9])", with, RegexOptions.IgnoreCase)
            : text;
    }

    // Any run of hex digits and colons that could be an address, kept only when Windows' own parser agrees it is one. That takes in the
    // short forms with "::", which a pattern alone gets wrong, and leaves times such as 12:30:45 and MAC-like pairs alone.
    private static bool IsIpv6(string candidate) =>
        candidate.Count(c => c == ':') >= 2 && IPAddress.TryParse(candidate, out var ip) && ip.AddressFamily == AddressFamily.InterNetworkV6;

    [GeneratedRegex(@"S-1-\d+(-\d+){2,}", RegexOptions.IgnoreCase)]
    private static partial Regex Sid();

    [GeneratedRegex(@"[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}")]
    private static partial Regex Email();

    [GeneratedRegex(@"\b[0-9A-Fa-f]{2}([:\-][0-9A-Fa-f]{2}){5}\b")]
    private static partial Regex Mac();

    [GeneratedRegex(@"(?<![0-9A-Za-z:])(?:[0-9A-Fa-f]{0,4}:){2,7}[0-9A-Fa-f]{0,4}(?![0-9A-Za-z:])")]
    private static partial Regex Ipv6Candidate();

    [GeneratedRegex(@"\b(?:\d{1,3}\.){3}\d{1,3}\b")]
    private static partial Regex Ipv4();

    // The folder name runs to the next backslash, so a name with a space, such as "Jane Doe", goes whole and not just its first word.
    [GeneratedRegex(@"([A-Za-z]:\\Users\\)[^\\/""'<>|\r\n]+", RegexOptions.IgnoreCase)]
    private static partial Regex UserFolder();

    // The name of another PC in a network path. Not matched after a word character or a colon, so a doubled backslash inside an escaped
    // path such as "C:\\Windows" is not taken for one.
    [GeneratedRegex(@"(?<![\\\w:.\-])\\\\[^\\\s""'/]+")]
    private static partial Regex UncServer();
}
