using System.Runtime.Versioning;
using Clarion.Core.Engine;
using Clarion.Core.Journal;
using Clarion.Core.Startup;
using Clarion.Engine;
using Microsoft.Win32;

namespace Clarion.Tests;

/// <summary>
/// Runs against the real registry with an entry it makes and removes itself. Skipped unless CLARION_REAL_TESTS=1. Needs an elevated shell.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class RealStartupTests : IDisposable
{
    private const string RunKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
    private readonly string _name = "ClarionTestStartup" + Guid.NewGuid().ToString("N")[..8];
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "clarion-real-startup-" + Guid.NewGuid().ToString("N"));

    private static bool Enabled =>
        OperatingSystem.IsWindows() && Environment.GetEnvironmentVariable("CLARION_REAL_TESTS") == "1" && WindowsMachine.IsElevated();

    public void Dispose()
    {
        using (var run = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)) run?.DeleteValue(_name, throwOnMissingValue: false);
        using (var approved = Registry.CurrentUser.OpenSubKey(StartupApproval.KeyOf(StartupOrigin.UserRun), writable: true))
            approved?.DeleteValue(_name, throwOnMissingValue: false);
        if (Directory.Exists(_dir)) Directory.Delete(_dir, true);
    }

    [Fact]
    public void A_real_entry_is_listed_turned_off_and_turned_back_on_exactly()
    {
        if (!Enabled) return;
        using (var run = Registry.CurrentUser.CreateSubKey(RunKey))
            run.SetValue(_name, "\"C:\\Windows\\System32\\notepad.exe\" /clarion-test");

        var source = new WindowsStartupSource();
        var before = StartupList.Build(source).Single(e => e.Name == _name);
        Assert.True(before.Enabled);
        Assert.Equal(StartupOrigin.UserRun, before.Origin);
        Assert.True(before.Program.Found);
        Assert.Contains("Microsoft", before.Program.Publisher, StringComparison.Ordinal);

        var runtime = EngineFactory.Create(_dir);
        var tweak = StartupTweaks.TurnOff(before);
        Assert.True(runtime.Engine.Apply(tweak, Guid.NewGuid()).Success);

        var off = StartupList.Build(new WindowsStartupSource()).Single(e => e.Name == _name);
        Assert.False(off.Enabled);
        Assert.NotNull(off.DisabledAt);
        // The entry itself is untouched.
        using (var run = Registry.CurrentUser.OpenSubKey(RunKey)) Assert.NotNull(run?.GetValue(_name));

        Assert.True(runtime.Engine.Revert(tweak, Guid.NewGuid()).Success);

        Assert.True(StartupList.Build(new WindowsStartupSource()).Single(e => e.Name == _name).Enabled);
        using var approved = Registry.CurrentUser.OpenSubKey(StartupApproval.KeyOf(StartupOrigin.UserRun));
        Assert.Null(approved?.GetValue(_name));
    }

    [Fact]
    public void The_list_on_this_pc_can_be_read_and_has_a_program_for_most_entries()
    {
        if (!Enabled) return;
        var list = StartupList.Build(new WindowsStartupSource());

        Assert.All(list, e => Assert.False(string.IsNullOrWhiteSpace(e.Name)));
        Assert.All(list, e => Assert.NotNull(e.Program));
    }
}
