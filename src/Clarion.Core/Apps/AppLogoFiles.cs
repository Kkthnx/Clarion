using System.Text.Json;
using System.Text.Json.Serialization;

namespace Clarion.Core.Apps;

/// <summary>Where one bundled logo picture came from, and what it was when it was saved. Plate says whether it was put on a white or a dark plate, or none.</summary>
public sealed record AppLogoFile(
    [property: JsonRequired] string Source,
    [property: JsonRequired] int Width,
    string Original,
    string Plate,
    [property: JsonRequired] string Sha256);

/// <summary>
/// The logos that are saved inside the program as picture files, for the apps that have no Simple Icons outline. Each was taken once, by
/// scripts/fetch-app-logos.ps1, from the project's own GitHub organization or its own web page, and this list says from where. Nothing is fetched while
/// the program runs.
/// </summary>
public sealed class AppLogoFiles
{
    /// <summary>The folder, beside the program, that holds the picture files.</summary>
    public const string FolderName = "AppLogos";

    private static readonly Lazy<AppLogoFiles> Embedded = new(LoadEmbedded);

    private readonly Dictionary<string, AppLogoFile> _files;

    private AppLogoFiles(Dictionary<string, AppLogoFile> files) => _files = files;

    public static AppLogoFiles Default => Embedded.Value;

    public IReadOnlyDictionary<string, AppLogoFile> All => _files;

    public AppLogoFile? For(string tweakId) => _files.GetValueOrDefault(tweakId);

    /// <summary>
    /// The file name for a catalog id. The dot in "app.name" is written as a hyphen, because the build reads whatever follows a dot in a file name
    /// as a resource qualifier and warns about every name it does not recognise.
    /// </summary>
    public static string FileNameFor(string tweakId) => tweakId.Replace('.', '-') + ".png";

    /// <summary>The picture file for a catalog id under the folder that holds them, or null when there is none or it is not on disk.</summary>
    public string? PathFor(string tweakId, string assetsFolder)
    {
        if (!_files.ContainsKey(tweakId)) return null;
        var path = System.IO.Path.Combine(assetsFolder, FolderName, FileNameFor(tweakId));
        return File.Exists(path) ? path : null;
    }

    public static AppLogoFiles Parse(Stream json) =>
        new(JsonSerializer.Deserialize<Dictionary<string, AppLogoFile>>(json, Catalog.CatalogLoader.Options) ?? throw new InvalidDataException("Empty logo file list"));

    private static AppLogoFiles LoadEmbedded()
    {
        var asm = typeof(AppLogoFiles).Assembly;
        var name = asm.GetManifestResourceNames().Single(n => n.EndsWith(".Catalog.Icons.app-logo-files.json", StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(name)!;
        return Parse(stream);
    }
}
