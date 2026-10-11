using System.Text.RegularExpressions;

namespace Clarion.Core.Engine;

/// <summary>
/// What a Windows service name may look like before it is put on a command line or into a registry path. Real names are letters,
/// digits and a few joining marks. A space would add arguments to sc.exe, and a backslash would climb out of the Services key.
/// </summary>
public static partial class ServiceNames
{
    [GeneratedRegex(@"^[A-Za-z0-9._-]{1,256}\z")]
    private static partial Regex Pattern();

    public static bool IsValid(string name) => Pattern().IsMatch(name);
}
