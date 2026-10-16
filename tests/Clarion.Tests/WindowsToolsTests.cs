using Clarion.Core.Actions;

namespace Clarion.Tests;

public sealed class WindowsToolsTests
{
    [Fact]
    public void Every_tool_has_a_distinct_name_and_a_sentence_that_says_what_it_is_for()
    {
        Assert.Equal(WindowsTools.All.Count, WindowsTools.All.Select(t => t.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(WindowsTools.All, t => Assert.EndsWith(".", t.Summary));
    }

    [Fact]
    public void Only_two_hosts_are_used_and_each_file_is_a_known_kind_without_a_folder()
    {
        Assert.All(WindowsTools.All, t =>
        {
            Assert.Contains(t.Host, new[] { "control.exe", "mmc.exe" });
            Assert.True(Path.GetExtension(t.File) is ".cpl" or ".msc", t.File);
            Assert.DoesNotContain('\\', t.File);
            Assert.DoesNotContain('/', t.File);
        });
    }

    [Fact]
    public void The_command_line_names_both_files_by_full_path_in_the_system_folder()
    {
        var tool = WindowsTools.All.First(t => t.File == "ncpa.cpl");
        var (program, arguments) = tool.CommandLine(@"C:\Windows\System32");
        Assert.Equal(@"C:\Windows\System32\control.exe", program);
        Assert.Equal("\"C:\\Windows\\System32\\ncpa.cpl\"", arguments);
    }

    [Fact]
    public void Every_file_exists_on_this_windows()
    {
        if (!OperatingSystem.IsWindows()) return;
        foreach (var t in WindowsTools.All)
        {
            var (program, _) = t.CommandLine(Environment.SystemDirectory);
            Assert.True(File.Exists(program), program);
            Assert.True(File.Exists(Path.Combine(Environment.SystemDirectory, t.File)), t.File);
        }
    }
}
