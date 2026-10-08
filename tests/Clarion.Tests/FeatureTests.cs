using System.Text;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Features;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Engine;

namespace Clarion.Tests;

public sealed class FakeFeatureStore : IFeatureStore
{
    private readonly Dictionary<string, bool> _features = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _caps = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Calls { get; } = [];

    public void Prefetch() { }
    public void AddFeature(string name, bool on) => _features[name] = on;
    public void AddCapability(string name, bool on) => _caps[name] = on;

    public bool? IsFeatureEnabled(string name) => _features.TryGetValue(name, out var v) ? v : null;
    public bool? IsCapabilityInstalled(string name) => _caps.TryGetValue(name, out var v) ? v : null;

    public void SetFeature(string name, bool enabled)
    {
        Calls.Add($"feature {name} {enabled}");
        _features[name] = enabled;
    }

    public void SetCapability(string name, bool installed)
    {
        Calls.Add($"capability {name} {installed}");
        _caps[name] = installed;
    }
}

public sealed class FeatureTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-feat-" + Guid.NewGuid().ToString("N"));
    private readonly FakeFeatureStore _store = new();
    private readonly TweakEngine _engine;

    public FeatureTests()
    {
        _engine = new TweakEngine([new FeatureHandler(_store)], new ChangeJournal(Path.Combine(_dir, "j.jsonl")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    private static Tweak Make(params Operation[] ops) => new()
    {
        Id = "feat.one", Category = "Features", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
        Evidence = Evidence.Situational, RiskLevel = RiskLevel.Low, Scope = TweakScope.Machine, Apply = ops,
    };

    [Fact]
    public void Turn_on_then_revert_returns_to_the_old_state()
    {
        _store.AddFeature("Microsoft-Windows-Subsystem-Linux", false);
        _store.AddFeature("VirtualMachinePlatform", false);
        var tweak = Make(new SetWindowsFeature("Microsoft-Windows-Subsystem-Linux", true), new SetWindowsFeature("VirtualMachinePlatform", true));

        Assert.Equal(TweakState.NotApplied, _engine.Detect(tweak));
        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.Equal(TweakState.Applied, _engine.Detect(tweak));

        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.False(_store.IsFeatureEnabled("Microsoft-Windows-Subsystem-Linux"));
        Assert.False(_store.IsFeatureEnabled("VirtualMachinePlatform"));
    }

    [Fact]
    public void A_feature_that_was_already_on_stays_on_after_revert()
    {
        _store.AddFeature("VirtualMachinePlatform", true);
        _store.AddFeature("Microsoft-Hyper-V-All", false);
        var tweak = Make(new SetWindowsFeature("VirtualMachinePlatform", true), new SetWindowsFeature("Microsoft-Hyper-V-All", true));

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.True(_store.IsFeatureEnabled("VirtualMachinePlatform"));
        Assert.False(_store.IsFeatureEnabled("Microsoft-Hyper-V-All"));
    }

    [Fact]
    public void Missing_features_are_skipped_and_all_missing_is_unavailable()
    {
        var gone = Make(new SetWindowsFeature("Containers-DisposableClientVM", true));
        Assert.Equal(TweakState.Unavailable, _engine.Detect(gone));
        Assert.False(_engine.Apply(gone, Guid.NewGuid()).Success);
        Assert.Empty(_store.Calls);
    }

    [Fact]
    public void Capability_install_and_revert()
    {
        _store.AddCapability("OpenSSH.Server~~~~0.0.1.0", false);
        var tweak = Make(new SetWindowsCapability("OpenSSH.Server~~~~0.0.1.0", true));

        Assert.True(_engine.Apply(tweak, Guid.NewGuid()).Success);
        Assert.True(_store.IsCapabilityInstalled("OpenSSH.Server~~~~0.0.1.0"));
        Assert.True(_engine.Revert(tweak, Guid.NewGuid()).Success);
        Assert.False(_store.IsCapabilityInstalled("OpenSSH.Server~~~~0.0.1.0"));
    }

    [Theory]
    [InlineData("Windows-Defender-Default-Definitions")]
    [InlineData("NetFx4-AdvSrvs")]
    public void Protected_features_are_never_turned_off(string name)
    {
        _store.AddFeature(name, true);
        var result = _engine.Apply(Make(new SetWindowsFeature(name, false)), Guid.NewGuid());
        Assert.False(result.Success);
        Assert.Contains("protected", result.Error);
        Assert.Empty(_store.Calls);
    }

    [Theory]
    [InlineData("Enabled", true)]
    [InlineData("EnablePending", true)]
    [InlineData("Disabled", false)]
    [InlineData("DisablePending", false)]
    [InlineData("DisabledWithPayloadRemoved", false)]
    [InlineData("PartiallyInstalled", false)]
    public void Dism_states_map_to_on_or_off(string state, bool expected)
    {
        Assert.Equal(expected, FeatureRules.IsEnabledState(state));
    }

    [Theory]
    [InlineData("a b")]
    [InlineData("x'; Stop-Process -Name explorer; '")]
    [InlineData("")]
    public void Unsafe_names_never_reach_a_script(string name)
    {
        Assert.False(FeatureRules.IsValidName(name));
        Assert.Throws<ArgumentException>(() => FeatureScripts.SetFeature(name, true));
        Assert.Throws<ArgumentException>(() => FeatureScripts.SetCapability(name, true));
        Assert.Throws<ArgumentException>(() => FeatureScripts.CapabilityState(name));
    }

    [Fact]
    public void Scripts_use_the_documented_switches_and_are_encoded()
    {
        var args = FeatureScripts.ToArguments(FeatureScripts.SetFeature("Microsoft-Hyper-V-All", true));
        var b64 = args[(args.IndexOf("-EncodedCommand ", StringComparison.Ordinal) + "-EncodedCommand ".Length)..];
        var on = Encoding.Unicode.GetString(Convert.FromBase64String(b64));
        Assert.Contains("Enable-WindowsOptionalFeature -Online -FeatureName 'Microsoft-Hyper-V-All' -All -NoRestart", on);
        Assert.Contains("Disable-WindowsOptionalFeature -Online -FeatureName 'X1' -NoRestart", FeatureScripts.SetFeature("X1", false));
    }

    [Fact]
    public void Inventory_json_parses()
    {
        var map = FeatureScripts.ParseStates("""{"features":[{"FeatureName":"NetFx3","State":"DisabledWithPayloadRemoved"},{"FeatureName":"MediaPlayback","State":"Enabled"}]}""", "features", "FeatureName");
        Assert.Equal("Enabled", map["mediaplayback"]);
        var caps = FeatureScripts.ParseStates("""{"capabilities":[{"Name":"OpenSSH.Server~~~~0.0.1.0","State":"NotPresent"}]}""", "capabilities", "Name");
        Assert.False(FeatureRules.IsInstalledState(caps["OpenSSH.Server~~~~0.0.1.0"]));
    }

    [Fact]
    public void Catalog_feature_entries_are_machine_scoped_and_safe()
    {
        var ops = CatalogLoader.LoadEmbedded().SelectMany(t => t.Apply).ToList();
        Assert.Contains(ops, o => o is SetWindowsFeature f && f.Name == "Microsoft-Windows-Subsystem-Linux");
        Assert.Contains(ops, o => o is SetWindowsCapability c && c.Name.StartsWith("OpenSSH.Server", StringComparison.Ordinal));
        Assert.Empty(CatalogLoader.Validate(CatalogLoader.LoadEmbedded()));
    }

    [Fact]
    public void Real_feature_inventory_runs_read_only()
    {
        if (!OperatingSystem.IsWindows()) return;
        var store = new WindowsFeatureStore(new WindowsProcessRunner());
        Assert.NotNull(store.IsFeatureEnabled("MediaPlayback"));
        Assert.Null(store.IsFeatureEnabled("NoSuchFeatureClarion"));
        Assert.NotNull(store.IsCapabilityInstalled("OpenSSH.Client~~~~0.0.1.0"));
    }
}
