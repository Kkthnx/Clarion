using Clarion.Core.Abstractions;
using Clarion.Core.Apps;
using Clarion.Engine;

namespace Clarion.Tests;

[System.Runtime.Versioning.SupportedOSPlatform("windows")]
public sealed class WingetRestartTests
{
    private sealed class Fixed(int exitCode) : IProcessRunner
    {
        public ProcessResult Run(string fileName, string arguments, TimeSpan timeout) => new(exitCode, "", "");
        public ProcessResult Run(string fileName, string arguments, TimeSpan timeout, CancellationToken cancel) => Run(fileName, arguments, timeout);
    }

    [Theory]
    [InlineData(unchecked((int)0x8A150109))]
    [InlineData(unchecked((int)0x8A15010B))]
    public void An_install_that_asks_for_a_restart_is_remembered_once(int code)
    {
        var store = new WindowsWingetStore(new Fixed(code), @"C:\Windows\System32\cmd.exe");

        var outcome = store.Install("7zip.7zip");

        Assert.True(outcome.Succeeded);
        Assert.True(store.TakeRestartNeeded());
        Assert.False(store.TakeRestartNeeded());
    }

    [Fact]
    public void A_plain_success_asks_for_nothing()
    {
        var store = new WindowsWingetStore(new Fixed(0), @"C:\Windows\System32\cmd.exe");
        store.Install("7zip.7zip");
        Assert.False(store.TakeRestartNeeded());
    }

    [Fact]
    public void An_uninstall_that_asks_for_a_restart_is_remembered_too()
    {
        var store = new WindowsWingetStore(new Fixed(unchecked((int)0x8A150109)), @"C:\Windows\System32\cmd.exe");
        store.Uninstall("7zip.7zip");
        Assert.True(store.TakeRestartNeeded());
    }
}
