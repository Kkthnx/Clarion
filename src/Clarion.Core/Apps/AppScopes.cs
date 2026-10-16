using System.Text.Json;
using Clarion.Core.Model;

namespace Clarion.Core.Apps;

/// <summary>Why an app may be installed for one account only: its manifest says so, or winget itself carries out the scope for its kind of package.</summary>
public sealed record AppScope(string Basis, string InstallerType);

/// <summary>
/// The apps that may be installed for the account that runs .Clarion only. winget accepting the option is not enough, because for an installer that
/// names no scope the flag is passed along and the installer may ignore it, which Microsoft's troubleshooting page says about EXE installers. An app is
/// listed only when winget picks an installer for it with the user scope and either its manifest declares that scope or winget carries it out itself
/// (portable and MSIX packages). scripts/check-winget-scopes.ps1 writes the list.
/// </summary>
public sealed class AppScopes
{
    private static readonly Lazy<AppScopes> Embedded = new(LoadEmbedded);

    private readonly Dictionary<string, AppScope> _scopes;

    private AppScopes(Dictionary<string, AppScope> scopes) => _scopes = scopes;

    public static AppScopes Default => Embedded.Value;

    public IReadOnlyDictionary<string, AppScope> All => _scopes;

    public bool SupportsUser(string tweakId) => _scopes.ContainsKey(tweakId);

    public AppScope? For(string tweakId) => _scopes.GetValueOrDefault(tweakId);

    public static AppScopes Parse(Stream json) =>
        new(JsonSerializer.Deserialize<Dictionary<string, AppScope>>(json, Catalog.CatalogLoader.Options) ?? throw new InvalidDataException("Empty scope file"));

    private static AppScopes LoadEmbedded()
    {
        var asm = typeof(AppScopes).Assembly;
        var name = asm.GetManifestResourceNames().Single(n => n.EndsWith(".Catalog.Apps.app-scopes.json", StringComparison.Ordinal));
        using var stream = asm.GetManifestResourceStream(name)!;
        return Parse(stream);
    }
}

/// <summary>
/// The "install for this account only" form of an app. It changes only the scope of the winget steps, and what Revert puts back is captured from
/// the step that ran, so removing it later is done for the same account.
/// </summary>
public static class UserScopeVariant
{
    /// <summary>True for an app that installs one winget package and is listed as able to go in one account.</summary>
    public static bool CanApply(Tweak tweak, AppScopes scopes) =>
        scopes.SupportsUser(tweak.Id) && tweak.Apply.OfType<InstallWingetPackage>().Any();

    public static Tweak Of(Tweak tweak) => tweak with
    {
        Apply = tweak.Apply.Select(o => o is InstallWingetPackage i ? i with { Scope = "user" } : o).ToList(),
        Undo = tweak.Undo.Select(o => o is UninstallWingetPackage u ? u with { Scope = "user" } : o).ToList(),
    };
}
