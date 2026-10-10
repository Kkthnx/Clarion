using Clarion.Core.Engine;

namespace Clarion.Tests;

public sealed class SystemToolsTests
{
    private const string System32 = @"C:\Windows\System32";

    [Theory]
    [InlineData("sc.exe", @"C:\Windows\System32\sc.exe")]
    [InlineData("SCHTASKS.EXE", @"C:\Windows\System32\SCHTASKS.EXE")]
    [InlineData("powercfg.exe", @"C:\Windows\System32\powercfg.exe")]
    public void A_windows_tool_is_taken_from_the_system_folder(string name, string expected) =>
        Assert.Equal(expected, SystemTools.Resolve(name, System32, _ => true));

    [Fact]
    public void Powershell_is_the_windows_powershell_in_its_own_folder()
    {
        Assert.Equal(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", SystemTools.Resolve("powershell.exe", System32, _ => true));
        // Where that file is somehow missing, the system folder is tried next, and then the plain name, never a guess elsewhere.
        Assert.Equal(@"C:\Windows\System32\powershell.exe", SystemTools.Resolve("powershell.exe", System32, p => p == @"C:\Windows\System32\powershell.exe"));
        Assert.Equal("powershell.exe", SystemTools.Resolve("powershell.exe", System32, _ => false));
    }

    [Theory]
    [InlineData(@"C:\Tools\thing.exe")]
    [InlineData(@"..\thing.exe")]
    [InlineData("sub/thing.exe")]
    public void A_name_that_already_has_a_folder_is_left_alone(string name) =>
        Assert.Equal(name, SystemTools.Resolve(name, System32, _ => true));

    [Fact]
    public void A_program_that_is_not_in_the_system_folder_keeps_its_name()
    {
        // Not looked up in the current folder or beside the program, because those are the places a planted file would be.
        Assert.Equal("unknown.exe", SystemTools.Resolve("unknown.exe", System32, _ => false));
    }

    [Fact]
    public void On_this_machine_every_tool_clarion_starts_by_name_is_found_in_the_system_folder()
    {
        if (!OperatingSystem.IsWindows()) return;
        foreach (var tool in new[] { "powershell.exe", "schtasks.exe", "sc.exe", "powercfg.exe" })
        {
            var path = SystemTools.Resolve(tool, Environment.SystemDirectory);
            Assert.True(Path.IsPathRooted(path), $"{tool} resolved to {path}");
            Assert.True(File.Exists(path), path);
            Assert.StartsWith(Environment.SystemDirectory, path, StringComparison.OrdinalIgnoreCase);
        }
    }
}
