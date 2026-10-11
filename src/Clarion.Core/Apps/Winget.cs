using System.Text.Json;
using System.Text.RegularExpressions;

namespace Clarion.Core.Apps;

/// <summary>What a winget package ID may look like before it is put on a command line.</summary>
public static partial class WingetIds
{
    // Publisher, a dot, then the package, as in 7zip.7zip or Notepad++.Notepad++. A space or a quote would end the argument and start another.
    [GeneratedRegex(@"^[A-Za-z0-9][A-Za-z0-9._+\-]{0,126}\.[A-Za-z0-9._+\-]{1,126}\z")]
    private static partial Regex Pattern();

    public static bool IsValid(string id) => id.Length <= 128 && Pattern().IsMatch(id);
}

/// <summary>
/// The install folders a catalog entry may give winget. Only a folder under Program Files or the person's own local app folder, written
/// with its variable, is accepted, so a quote, a dot-dot or another drive can never reach the command line.
/// </summary>
public static partial class WingetLocations
{
    [GeneratedRegex(@"^%(ProgramFiles|ProgramFiles\(x86\)|LocalAppData)%(\\[A-Za-z0-9][A-Za-z0-9 ._()\-]{0,60}){1,3}\z")]
    private static partial Regex Pattern();

    public static bool IsValid(string location) => location.Length <= 200 && Pattern().IsMatch(location);

    /// <summary>The folder with its variable filled in, for this PC and account.</summary>
    public static string Expand(string location, Func<string, string?>? variable = null)
    {
        if (!IsValid(location)) throw new ArgumentException($"Not an allowed install folder: {location}");
        variable ??= Environment.GetEnvironmentVariable;
        var name = location[1..location.IndexOf('%', 1)];
        var root = variable(name);
        if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException($"{name} is not set on this PC.");
        return root.TrimEnd('\\') + location[(name.Length + 2)..];
    }
}

/// <summary>
/// The exact winget command lines Clarion runs. Every install is pinned by ID with --exact and the winget source, because without --exact
/// winget matches the text anywhere in a name, ID or moniker. Nothing else is ever built from user text.
/// </summary>
public static class WingetCommands
{
    private const string Common = "--source winget --accept-source-agreements --disable-interactivity";

    public static string Install(string id, string? location = null) =>
        $"install --id {Checked(id)} --exact {Common} --silent --accept-package-agreements{LocationArgument(location)}";

    // A package whose manifest says InstallLocationRequired fails with 0x8A15005F unless --location is given.
    private static string LocationArgument(string? location) =>
        location is null ? "" : $" --location \"{WingetLocations.Expand(location)}\"";

    public static string Uninstall(string id) =>
        $"uninstall --id {Checked(id)} --exact {Common} --silent";

    public static string Show(string id) =>
        $"show --id {Checked(id)} --exact {Common}";

    /// <summary>Writes every installed package winget can match to its source, as JSON, to a file.</summary>
    public static string Export(string path) =>
        path.Contains('"') ? throw new ArgumentException("Not a valid file path.") : $"export --output \"{path}\" {Common}";

    private static string Checked(string id) =>
        WingetIds.IsValid(id) ? id : throw new ArgumentException($"Not a valid winget package ID: {id}");
}

/// <summary>What winget's documented return codes mean, in plain words. The codes are from the winget-cli repository's return code list.</summary>
public static class WingetCodes
{
    public const int Success = 0;
    public const int NoPackagesFound = unchecked((int)0x8A150014);
    private const int AlreadyInstalledA = unchecked((int)0x8A150061);
    private const int AlreadyInstalledB = unchecked((int)0x8A15010D);
    private const int HigherVersionInstalled = unchecked((int)0x8A15010E);
    private const int RebootToFinish = unchecked((int)0x8A150109);
    private const int RebootInitiated = unchecked((int)0x8A15010B);

    /// <summary>True when the package is installed now, whichever way that came about.</summary>
    public static bool IsSuccess(int code) =>
        code is Success or AlreadyInstalledA or AlreadyInstalledB or HigherVersionInstalled or RebootToFinish or RebootInitiated;

    public static bool NeedsRestart(int code) => code is RebootToFinish or RebootInitiated;

    public static string Explain(int code) => code switch
    {
        Success => "Done.",
        AlreadyInstalledA or AlreadyInstalledB => "It was already installed.",
        HigherVersionInstalled => "A newer version is already installed.",
        RebootToFinish or RebootInitiated => "Done. Restart the PC to finish.",
        NoPackagesFound => "winget found no matching package.",
        unchecked((int)0x8A150010) => "winget has no installer that fits this PC, for example for its processor or Windows version.",
        unchecked((int)0x8A150011) => "The download did not match its checksum, so winget stopped and nothing was installed.",
        unchecked((int)0x8A15002D) => "The installer failed winget's security check, so nothing was installed.",
        unchecked((int)0x8A15002E) => "The download was not the size it should be, so winget stopped.",
        unchecked((int)0x8A150041) => "The licence terms were not accepted, so nothing was installed.",
        unchecked((int)0x8A150046) => "winget's source terms were not accepted, so it could not continue.",
        unchecked((int)0x8A15003A) or unchecked((int)0x8A15010F) => "A Group Policy on this PC blocks it.",
        unchecked((int)0x8A150101) => "The app is running. Close it and try again.",
        unchecked((int)0x8A150103) => "Some of its files are in use. Close the app and try again.",
        unchecked((int)0x8A150102) => "Another installation is running. Try again when it finishes.",
        unchecked((int)0x8A150104) => "It needs something that is missing from this PC.",
        unchecked((int)0x8A150105) => "There is not enough space on the disk.",
        unchecked((int)0x8A150106) => "There is not enough memory free. Close other programs and try again.",
        unchecked((int)0x8A150107) => "It needs an internet connection.",
        unchecked((int)0x8A15010A) => "Restart the PC and try again.",
        unchecked((int)0x8A15010C) => "The installation was cancelled.",
        unchecked((int)0x8A150108) or unchecked((int)0x8A150115) => "The installer reported an error of its own.",
        unchecked((int)0x8A150113) => "This package does not support this PC.",
        unchecked((int)0x8A150110) => "winget could not install what it needs first.",
        unchecked((int)0x8A15002F) => "winget has no way to uninstall it.",
        unchecked((int)0x8A150030) => "Its uninstaller did not finish.",
        unchecked((int)0x8A15000B) or unchecked((int)0x8A15004B) or unchecked((int)0x8A150045) or unchecked((int)0x8A150015) =>
            "winget could not open its package source. Check the internet connection, then try again.",
        unchecked((int)0x8A15005F) => "This app's installer cannot run without an install folder, and none is set for it here.",
        unchecked((int)0x8A150056) => "This app's installer refuses to run with administrator rights, which Clarion needs for apps.",
        unchecked((int)0x8A150008) => "The download failed. Check the internet connection, then try again.",
        unchecked((int)0x8A150086) => "The download came back empty. Check the internet connection, then try again.",
        unchecked((int)0x8A150049) => "The Windows Installer part of the setup failed.",
        unchecked((int)0x8A150052) => "winget could not set up this portable app.",
        unchecked((int)0x8A15006D) => "A service winget needs is busy. Try again in a few minutes.",
        unchecked((int)0x8A150005) => "It was stopped.",
        -1 => "winget took too long, so it was stopped.",
        -2 => "Stopped before it finished.",
        _ => $"winget stopped with code 0x{code:X8}. Its log has the detail.",
    };
}

/// <summary>What <c>winget show</c> says about a package, read from its text output.</summary>
public sealed record WingetPackageInfo(
    string Id, string Name, string Version, string Publisher, string License, string LicenseUrl, string Homepage,
    string InstallerType, string InstallerUrl, string Sha256)
{
    /// <summary>The site the installer is downloaded from, so the person sees who serves it.</summary>
    public string InstallerHost => Uri.TryCreate(InstallerUrl, UriKind.Absolute, out var u) ? u.Host : "";
}

public static partial class WingetShow
{
    [GeneratedRegex(@"\[([^\]\s]+)\]")]
    private static partial Regex IdInBrackets();

    [GeneratedRegex(@"^[0-9a-fA-F]{64}\z")]
    private static partial Regex Sha256();

    /// <summary>
    /// Reads the text <c>winget show</c> prints. The labels are in the language Windows displays, so the installer block is the one that holds
    /// a 64 character checksum, and its address and checksum are found by what they look like. The other fields are found by their English
    /// labels, and a field whose label is not recognised comes back empty. Null when the output is not a package record.
    /// </summary>
    public static WingetPackageInfo? Parse(string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n');
        var header = lines.Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0 && IdInBrackets().IsMatch(l));
        if (header is null) return null;

        var idMatch = IdInBrackets().Match(header);
        var id = idMatch.Groups[1].Value;
        var name = header[..idMatch.Index].Trim();
        // The first word is "Found" in English and something else elsewhere, so it is dropped by position, not by spelling.
        var firstSpace = name.IndexOf(' ');
        if (firstSpace > 0) name = name[(firstSpace + 1)..].Trim();

        var top = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var blocks = new List<List<(string Key, string Value)>>();
        List<(string Key, string Value)>? current = null;
        foreach (var raw in lines.Skip(1))
        {
            if (raw.Trim().Length == 0) continue;
            var indented = char.IsWhiteSpace(raw[0]);
            var line = raw.Trim();
            var colon = line.IndexOf(": ", StringComparison.Ordinal);

            if (!indented)
            {
                if (line.EndsWith(':')) { current = []; blocks.Add(current); }
                else if (colon > 0) { top[line[..colon]] = line[(colon + 2)..].Trim(); current = null; }
                continue;
            }
            if (current is not null && colon > 0) current.Add((line[..colon], line[(colon + 2)..].Trim()));
        }

        var installer = blocks.FirstOrDefault(b => b.Any(p => Sha256().IsMatch(p.Value)));
        if (installer is null) return null;

        var sha = installer.First(p => Sha256().IsMatch(p.Value)).Value;
        var url = installer.Select(p => p.Value).FirstOrDefault(v => v.StartsWith("http", StringComparison.OrdinalIgnoreCase)) ?? "";
        var type = installer.FirstOrDefault(p => p.Key.Equals("Installer Type", StringComparison.OrdinalIgnoreCase)).Value ?? "";

        string Get(string key) => top.TryGetValue(key, out var v) ? v : "";
        return new WingetPackageInfo(id, name, Get("Version"), Get("Publisher"), Get("License"), Get("License Url"), Get("Homepage"), type, url, sha.ToLowerInvariant());
    }
}


/// <summary>Reads the file <c>winget export</c> writes.</summary>
public static class WingetExport
{
    public static IReadOnlySet<string> InstalledIds(string json)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("Sources", out var sources) || sources.ValueKind != JsonValueKind.Array) return ids;
        foreach (var source in sources.EnumerateArray())
        {
            if (!source.TryGetProperty("Packages", out var packages) || packages.ValueKind != JsonValueKind.Array) continue;
            foreach (var package in packages.EnumerateArray())
                if (package.TryGetProperty("PackageIdentifier", out var id) && id.GetString() is { Length: > 0 } text) ids.Add(text);
        }
        return ids;
    }
}

/// <summary>What the apps page needs from winget. Kept small so the handler can be tested without it.</summary>
public interface IWingetStore
{
    /// <summary>False when winget cannot be found for this account.</summary>
    bool IsAvailable { get; }

    /// <summary>True once the person has accepted the licence terms of the packages about to be installed.</summary>
    bool AcceptPackageAgreements { get; set; }

    /// <summary>The IDs of the installed packages winget can match to its community source. Throws when winget cannot answer.</summary>
    IReadOnlySet<string> InstalledIds();

    /// <summary>Forgets the installed list, so the next read asks winget again.</summary>
    void Invalidate();

    /// <summary>Installs the package. The location is passed only when the package's manifest requires one.</summary>
    WingetOutcome Install(string id, string? location = null);
    WingetOutcome Uninstall(string id);

    /// <summary>Stops the install or uninstall that is running now, with the installer winget started. Does nothing when none is running.</summary>
    void CancelRunning();

    /// <summary>The version, publisher, licence and installer details winget holds for a package, or null when it cannot be read.</summary>
    WingetPackageInfo? Show(string id);
}

public sealed partial record WingetOutcome(int ExitCode, string Output)
{
    // winget prints this line when the installer itself failed. Only the English wording is known, so in another language nothing is added.
    [GeneratedRegex(@"Installer failed with exit code:\s*(-?\d{1,10})(?!\d)")]
    private static partial Regex InstallerCode();

    public bool Succeeded => WingetCodes.IsSuccess(ExitCode);

    /// <summary>What happened, in a sentence, with the installer's own exit code when winget printed one, which is what a search for the error needs.</summary>
    public string Message
    {
        get
        {
            var sentence = WingetCodes.Explain(ExitCode);
            var match = Succeeded ? Match.Empty : InstallerCode().Match(Output);
            return match.Success ? $"{sentence} The installer reported exit code {match.Groups[1].Value}." : sentence;
        }
    }
}
