using System.Reflection;
using Clarion.Core.Reports;

// Writes the documentation site for GitHub Pages into a folder.
// Usage: dotnet run --project tools/Clarion.Site -- <output folder>
if (args.Length != 1)
{
    Console.Error.WriteLine("Usage: Clarion.Site <output folder>");
    return 2;
}

var output = Path.GetFullPath(args[0]);
var root = FindRepositoryRoot();
if (root is null)
{
    Console.Error.WriteLine("Run this from inside the repository, so the brand images can be found.");
    return 2;
}

var version = typeof(SiteSource).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
var files = CatalogSite.Build(SiteSource.FromCatalog(version));

if (Directory.Exists(output)) Directory.Delete(output, recursive: true);
foreach (var (path, text) in files)
{
    var target = Path.Combine(output, path.Replace('/', Path.DirectorySeparatorChar));
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    File.WriteAllText(target, text);
}

// Pages runs the files through Jekyll unless this marker is present, and nothing here needs it.
File.WriteAllText(Path.Combine(output, ".nojekyll"), "");
var assets = Path.Combine(output, "assets");
File.Copy(Path.Combine(root, "docs", "brand", "app-icon-1024.png"), Path.Combine(assets, "icon.png"));
File.Copy(Path.Combine(root, "docs", "brand", "banner-1280x640.png"), Path.Combine(assets, "banner.png"));

Console.WriteLine($"Wrote {files.Count} files and 2 images to {output}");
return 0;

static string? FindRepositoryRoot()
{
    for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir is not null; dir = dir.Parent)
        if (File.Exists(Path.Combine(dir.FullName, "Clarion.sln"))) return dir.FullName;
    return null;
}
