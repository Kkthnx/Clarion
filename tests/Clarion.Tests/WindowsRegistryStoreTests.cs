using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Model;
using Clarion.Engine;
using Microsoft.Win32;
using RegistryHive = Clarion.Core.Model.RegistryHive;

namespace Clarion.Tests;

/// <summary>Runs against a throwaway key under HKCU. Skipped on other platforms.</summary>
public sealed class WindowsRegistryStoreTests : IDisposable
{
    private readonly string _root = "Software\\ClarionTests-" + Guid.NewGuid().ToString("N");

    public void Dispose()
    {
        if (OperatingSystem.IsWindows()) Registry.CurrentUser.DeleteSubKeyTree(_root, throwOnMissingSubKey: false);
    }

    [Fact]
    public void Real_registry_round_trip_for_every_value_kind()
    {
        if (!OperatingSystem.IsWindows()) return;
        var store = new WindowsRegistryStore();
        var path = _root + "\\Kinds";

        var cases = new (string Name, RegistryData Data)[]
        {
            ("D", new RegistryData(RegistryKind.DWord, "4294967295")),
            ("Q", new RegistryData(RegistryKind.QWord, "18446744073709551615")),
            ("S", new RegistryData(RegistryKind.String, "hello")),
            ("E", new RegistryData(RegistryKind.ExpandString, "%TEMP%\\x")),
            ("B", new RegistryData(RegistryKind.Binary, "DEADBEEF")),
            ("", new RegistryData(RegistryKind.String, "default")),
        };

        foreach (var (name, data) in cases)
        {
            var target = new RegistryTarget(RegistryHive.CurrentUser, path, name);
            store.Write(target, data);
            var read = store.Read(target);
            Assert.True(read.Exists);
            Assert.Equal(data, read.Data);
        }
    }

    [Fact]
    public void Engine_apply_and_revert_on_real_registry_leaves_no_trace()
    {
        if (!OperatingSystem.IsWindows()) return;
        var dir = Path.Combine(Path.GetTempPath(), "clarion-real-" + Guid.NewGuid().ToString("N"));
        try
        {
            var engine = new TweakEngine(new WindowsRegistryStore(), new ChangeJournal(Path.Combine(dir, "j.jsonl")));
            var target = new RegistryTarget(RegistryHive.CurrentUser, _root + "\\Created\\Deep", "");
            var tweak = new Tweak
            {
                Id = "real.one", Category = "T", Name = "n", Summary = "s", What = "w", Benefit = "b", Risk = "r",
                Evidence = Evidence.Cosmetic, RiskLevel = RiskLevel.Safe,
                Apply = [new SetRegistryValue(target, new RegistryData(RegistryKind.String, ""))],
            };

            Assert.True(engine.Apply(tweak, Guid.NewGuid()).Success);
            Assert.Equal(TweakState.Applied, engine.Detect(tweak));
            Assert.True(engine.Revert(tweak, Guid.NewGuid()).Success);

            using var created = Registry.CurrentUser.OpenSubKey(_root + "\\Created");
            Assert.Null(created);
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }
}
