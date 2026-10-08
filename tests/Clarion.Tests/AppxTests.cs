using System.Text;
using Clarion.Core.Appx;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class FakeAppxStore : IAppxStore
{
    public List<AppxPackageInfo> Installed { get; } = [];
    public HashSet<string> Current { get; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Provisioned { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Calls { get; } = [];

    public void Add(string name, string signature = "Store", bool nonRemovable = false, bool framework = false, bool provisioned = true)
    {
        Installed.Add(new AppxPackageInfo(name, name + "_1.0_x64__abc", name + "_abc", nonRemovable, framework, signature));
        Current.Add(name);
        if (provisioned) Provisioned.Add(name);
    }

    public AppxSnapshot GetSnapshot() => new(Installed.ToList(), Current.ToHashSet(StringComparer.OrdinalIgnoreCase), Provisioned.ToHashSet(StringComparer.OrdinalIgnoreCase));

    public void Remove(string name, bool allUsers, bool deprovision)
    {
        Calls.Add($"remove {name} {allUsers} {deprovision}");
        Current.Remove(name);
        if (allUsers) Installed.RemoveAll(p => p.Name == name);
        if (deprovision) Provisioned.Remove(name);
    }

    public void Restore(string familyName)
    {
        Calls.Add($"restore {familyName}");
        var name = familyName[..familyName.LastIndexOf('_')];
        Current.Add(name);
        if (!Installed.Any(p => p.Name == name)) Installed.Add(new AppxPackageInfo(name, name + "_1.0", familyName, false, false, "Store"));
    }
}

public sealed class AppxTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-appx-" + Guid.NewGuid().ToString("N"));
    private readonly FakeAppxStore _store = new();
    private readonly TweakEngine _engine;

    public AppxTests()
    {
        _engine = new TweakEngine([new AppxHandler(_store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak Make(string name, bool all = false, bool deprov = false) => new()
    {
        Id = "app." + name, Category = "Debloat", Name = name, Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Situational, RiskLevel = RiskLevel.Low,
        Scope = all || deprov ? TweakScope.Machine : TweakScope.User,
        Apply = [new RemoveAppxPackage(name, all, deprov)],
    };

    [Fact]
    public void Remove_then_revert_restores_by_family_name()
    {
        _store.Add("Contoso.Widget");
        var tweak = Make("Contoso.Widget");

        Assert.Equal(TweakState.NotApplied, _engine.Detect(tweak));
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));
        Assert.DoesNotContain("Contoso.Widget", _store.Current);

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.Contains("restore Contoso.Widget_abc", _store.Calls);
        Assert.Contains("Contoso.Widget", _store.Current);
    }

    [Fact]
    public void Not_installed_reads_as_already_applied()
    {
        var tweak = Make("Contoso.Gone");
        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Empty(_store.Calls);
    }

    [Theory]
    [InlineData("Microsoft.WindowsStore")]
    [InlineData("Microsoft.VCLibs.140.00")]
    [InlineData("Microsoft.UI.Xaml.2.8")]
    [InlineData("MicrosoftWindows.Client.CBS")]
    [InlineData("Microsoft.SecHealthUI")]
    public void Protected_packages_are_refused(string name)
    {
        _store.Add(name);
        var result = _engine.Apply(Make(name), Guid.NewGuid());
        Assert.False(result.Success);
        Assert.Contains("protected", result.Error);
        Assert.Empty(_store.Calls);
    }

    [Theory]
    [InlineData("System", false, false)]
    [InlineData("Store", true, false)]
    [InlineData("Store", false, true)]
    public void Windows_owned_packages_are_refused(string signature, bool nonRemovable, bool framework)
    {
        _store.Add("Contoso.Core", signature, nonRemovable, framework);
        var result = _engine.Apply(Make("Contoso.Core"), Guid.NewGuid());
        Assert.False(result.Success);
        Assert.Contains("owned by Windows", result.Error);
    }

    [Fact]
    public void All_accounts_and_new_accounts_flags_are_passed_through()
    {
        _store.Add("Contoso.Widget");
        Assert.True(_engine.Apply(Make("Contoso.Widget", all: true, deprov: true), Guid.NewGuid()).Success);
        Assert.Contains("remove Contoso.Widget True True", _store.Calls);
        Assert.DoesNotContain("Contoso.Widget", _store.Provisioned);
    }

    [Theory]
    [InlineData("x'; Stop-Process -Name explorer; '")]
    [InlineData("a b")]
    [InlineData("")]
    [InlineData("*")]
    public void Unsafe_names_never_reach_a_script(string name)
    {
        Assert.False(AppxSafety.IsValidName(name));
        Assert.Throws<ArgumentException>(() => AppxScripts.Remove(name, false, false));
        Assert.Throws<ArgumentException>(() => AppxScripts.Restore(name));
    }

    [Fact]
    public void Scripts_are_encoded_and_decode_to_the_expected_command()
    {
        var args = AppxScripts.ToArguments(AppxScripts.Remove("Contoso.Widget", true, true));
        Assert.Contains("-EncodedCommand", args);
        Assert.DoesNotContain("Contoso", args);

        var b64 = args[(args.IndexOf("-EncodedCommand", StringComparison.Ordinal) + "-EncodedCommand ".Length)..];
        var script = Encoding.Unicode.GetString(Convert.FromBase64String(b64));
        Assert.Contains("Remove-AppxPackage -AllUsers", script);
        Assert.Contains("Remove-AppxProvisionedPackage -Online", script);
        Assert.Contains("'Contoso.Widget'", script);
    }

    [Fact]
    public void Snapshot_parses_inventory_json()
    {
        const string json = """
        {"installed":[{"Name":"A.One","PackageFullName":"A.One_1_x64__x","PackageFamilyName":"A.One_x","NonRemovable":false,"IsFramework":false,"SignatureKind":"Store"}],
         "current":["A.One"],"provisioned":["A.One","B.Two"]}
        """;
        var snap = AppxSnapshot.Parse(json);
        Assert.True(snap.IsInstalledForAnyone("a.one"));
        Assert.True(snap.IsInstalledForCurrentUser("A.One"));
        Assert.True(snap.IsProvisioned("B.Two"));
        Assert.Equal("A.One_x", snap.Find("A.One")!.FamilyName);
    }

    [Fact]
    public void Snapshot_parses_empty_lists()
    {
        var snap = AppxSnapshot.Parse("""{"installed":[],"current":[],"provisioned":[]}""");
        Assert.Empty(snap.Installed);
        Assert.False(snap.IsInstalledForAnyone("x"));
    }

    [Fact]
    public void Catalog_validator_rejects_protected_and_scope_mistakes()
    {
        const string json = """
        [{
          "id": "bad.app", "category": "x", "name": "n", "summary": "s", "what": "w", "benefit": "b", "risk": "r",
          "evidence": "Cosmetic", "riskLevel": "Low", "scope": "User", "advice": "a", "facts": ["one","two"],
          "apply": [
            { "type": "appx.remove", "name": "Microsoft.WindowsStore" },
            { "type": "appx.remove", "name": "Contoso.Widget", "allUsers": true }
          ]
        }]
        """;
        var errors = CatalogLoader.Validate(CatalogLoader.Parse(json));
        Assert.Contains(errors, e => e.Contains("protected package"));
        Assert.Contains(errors, e => e.Contains("needs Machine scope"));
    }

    [Fact]
    public void Debloat_catalog_entries_never_target_protected_names()
    {
        var apps = CatalogLoader.LoadEmbedded().SelectMany(t => t.Apply.OfType<RemoveAppxPackage>()).ToList();
        Assert.True(apps.Count >= 20);
        Assert.All(apps, a => Assert.False(AppxSafety.IsProtected(a.Name), a.Name));
    }
}
