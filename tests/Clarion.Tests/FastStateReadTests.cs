using System.Runtime.Versioning;
using Clarion.Core.Catalog;
using Clarion.Core.Engine;
using Clarion.Core.Features;
using Clarion.Core.Model;
using Clarion.Engine;
using Xunit.Abstractions;

namespace Clarion.Tests;

public sealed class FeatureStateNameTests
{
    // The numbers are DismPackageFeatureState in Microsoft's DISM API reference. The names are the ones Get-WindowsOptionalFeature prints.
    [Theory]
    [InlineData(0, "NotPresent")]
    [InlineData(1, "DisablePending")]
    [InlineData(2, "Disabled")]
    [InlineData(3, "DisabledWithPayloadRemoved")]
    [InlineData(4, "Enabled")]
    [InlineData(5, "EnablePending")]
    [InlineData(7, "PartiallyInstalled")]
    public void A_feature_state_number_gets_the_name_PowerShell_uses(int number, string name) =>
        Assert.Equal(name, FeatureRules.FeatureStateName(number));

    [Fact]
    public void Only_installed_and_pending_installs_count_as_on_for_a_feature()
    {
        var on = Enumerable.Range(0, 8).Where(n => FeatureRules.IsEnabledState(FeatureRules.FeatureStateName(n))).ToList();
        Assert.Equal([4, 5], on);
    }

    [Theory]
    [InlineData(0, "NotPresent")]
    [InlineData(2, "Staged")]
    [InlineData(4, "Installed")]
    public void A_capability_state_number_gets_the_name_PowerShell_uses(int number, string name) =>
        Assert.Equal(name, FeatureRules.CapabilityStateName(number));

    [Fact]
    public void Only_installed_counts_as_installed_for_a_capability()
    {
        var on = Enumerable.Range(0, 8).Where(n => FeatureRules.IsInstalledState(FeatureRules.CapabilityStateName(n))).ToList();
        Assert.Equal([4], on);
    }

    [Fact]
    public void A_number_nobody_documented_is_unknown_and_never_on()
    {
        Assert.Equal("Unknown", FeatureRules.FeatureStateName(99));
        Assert.False(FeatureRules.IsEnabledState(FeatureRules.FeatureStateName(99)));
        Assert.False(FeatureRules.IsInstalledState(FeatureRules.CapabilityStateName(99)));
    }
}

/// <summary>
/// Checks the fast reads against the PowerShell cmdlets on the real machine. Skipped unless CLARION_REAL_TESTS=1, and the DISM API needs
/// an elevated shell.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class FastStateReadRealTests(ITestOutputHelper output)
{
    private static bool Enabled =>
        OperatingSystem.IsWindows() && Environment.GetEnvironmentVariable("CLARION_REAL_TESTS") == "1" && WindowsMachine.IsElevated();

    [Fact]
    public void Every_catalog_feature_reads_the_same_through_the_API_as_through_PowerShell()
    {
        if (!Enabled) return;
        var runner = new WindowsProcessRunner();
        var store = new WindowsFeatureStore(runner);
        var names = CatalogLoader.LoadEmbedded().SelectMany(t => t.Apply).OfType<SetWindowsFeature>().Select(o => o.Name).Distinct().ToList();
        Assert.NotEmpty(names);

        var viaScript = FeatureScripts.ParseStates(PowerShellHost.Run(runner, FeatureScripts.FeatureStates(names), TimeSpan.FromMinutes(5)), "features", "FeatureName");
        foreach (var name in names)
        {
            bool? expected = viaScript.TryGetValue(name, out var state) ? FeatureRules.IsEnabledState(state) : null;
            output.WriteLine($"{name}: api={store.IsFeatureEnabled(name)} powershell={expected}");
            Assert.Equal(expected, store.IsFeatureEnabled(name));
        }
    }

    [Fact]
    public void A_feature_this_PC_does_not_have_reads_as_missing_not_as_off()
    {
        if (!Enabled) return;
        Assert.Null(new WindowsFeatureStore(new WindowsProcessRunner()).IsFeatureEnabled("Clarion-No-Such-Feature"));
    }

    [Fact]
    public void The_adapters_read_the_same_as_Get_NetAdapter()
    {
        if (!Enabled) return;
        var runner = new WindowsProcessRunner();
        var viaCim = new WindowsDnsStore(runner).GetAdapters();
        var script = "$a=@(Get-NetAdapter | Where-Object { $_.Status -eq 'Up' -and $_.HardwareInterface } | ForEach-Object { [int]$_.InterfaceIndex }); ConvertTo-Json -InputObject @{i=$a} -Compress";
        using var doc = System.Text.Json.JsonDocument.Parse(PowerShellHost.Run(runner, script, TimeSpan.FromMinutes(1)));
        var expected = doc.RootElement.GetProperty("i").EnumerateArray().Select(e => e.GetInt32()).Order().ToList();
        Assert.Equal(expected, viaCim.Select(a => a.IfIndex).Order().ToList());
    }
}
