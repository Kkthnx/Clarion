using Clarion.Core.Engine;
using Clarion.Core.Model;

namespace Clarion.Tests;

public sealed class RestorePointAccessTests
{
    private static readonly RegistryTarget Frequency = new(RegistryHive.LocalMachine,
        "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\SystemRestore", "SystemRestorePointCreationFrequency");

    [Fact]
    public void Without_registry_rights_making_a_restore_point_reports_it_instead_of_throwing()
    {
        // A copy running without administrator rights cannot write under HKEY_LOCAL_MACHINE.
        var registry = new FakeRegistry { FailWrite = _ => true };
        var runner = new FakeProcessRunner();

        var result = new RestorePointService(registry, runner).Create("by hand");

        Assert.False(result.Success);
        Assert.Contains("administrator", result.Error, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(runner.Calls);
    }

    [Fact]
    public void A_value_that_was_never_changed_is_not_touched_when_the_first_write_fails()
    {
        var registry = new FakeRegistry();
        registry.Write(Frequency, new RegistryData(RegistryKind.DWord, "1440"));
        registry.FailWrite = _ => true;

        var result = new RestorePointService(registry, new FakeProcessRunner()).Create("by hand");

        Assert.False(result.Success);
        registry.FailWrite = null;
        Assert.Equal("1440", registry.Read(Frequency).Data!.Value);
    }

    [Fact]
    public void The_frequency_value_is_put_back_after_a_successful_restore_point()
    {
        var registry = new FakeRegistry();
        registry.Write(Frequency, new RegistryData(RegistryKind.DWord, "1440"));

        var result = new RestorePointService(registry, new FakeProcessRunner()).Create("by hand");

        Assert.True(result.Success);
        Assert.Equal("1440", registry.Read(Frequency).Data!.Value);
    }
}
