using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Clarion.Core.Abstractions;
using Clarion.Core.Apps;
using Clarion.Engine;

namespace Clarion.Tests;

[SupportedOSPlatform("windows")]
public sealed class StopAndLaunchTests
{
    private static readonly string Cmd = Path.Combine(Environment.SystemDirectory, "cmd.exe");

    [Fact]
    public void A_cancelled_run_stops_the_program_and_says_so()
    {
        var runner = new WindowsProcessRunner();
        using var cancel = new CancellationTokenSource();
        cancel.CancelAfter(TimeSpan.FromMilliseconds(600));
        var clock = Stopwatch.StartNew();

        var result = runner.Run(Cmd, "/c ping -n 40 127.0.0.1", TimeSpan.FromMinutes(2), cancel.Token);

        Assert.Equal(-2, result.ExitCode);
        Assert.True(clock.Elapsed < TimeSpan.FromSeconds(10), $"took {clock.Elapsed}");
    }

    [Fact]
    public void A_run_that_takes_too_long_is_stopped_and_says_so()
    {
        var result = new WindowsProcessRunner().Run(Cmd, "/c ping -n 40 127.0.0.1", TimeSpan.FromMilliseconds(800));
        Assert.Equal(-1, result.ExitCode);
    }

    [Fact]
    public void A_run_that_finishes_is_not_disturbed_by_an_unused_token()
    {
        using var cancel = new CancellationTokenSource();
        var result = new WindowsProcessRunner().Run(Cmd, "/c echo hello", TimeSpan.FromMinutes(1), cancel.Token);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("hello", result.Output, StringComparison.Ordinal);
    }

    private sealed class WaitingRunner : IProcessRunner
    {
        public ManualResetEventSlim Started { get; } = new();

        public ProcessResult Run(string fileName, string arguments, TimeSpan timeout) => Run(fileName, arguments, timeout, CancellationToken.None);

        public ProcessResult Run(string fileName, string arguments, TimeSpan timeout, CancellationToken cancel)
        {
            Started.Set();
            return cancel.WaitHandle.WaitOne(TimeSpan.FromSeconds(20)) ? new ProcessResult(-2, "", "stopped") : new ProcessResult(0, "", "");
        }
    }

    [Fact]
    public async Task Stopping_the_store_stops_the_install_that_is_running()
    {
        var runner = new WaitingRunner();
        var store = new WindowsWingetStore(runner, Cmd) { AcceptPackageAgreements = true };

        var install = Task.Run(() => store.Install("7zip.7zip"));
        Assert.True(runner.Started.Wait(TimeSpan.FromSeconds(5)));
        store.CancelRunning();

        var outcome = await install.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal(-2, outcome.ExitCode);
        Assert.False(outcome.Succeeded);
        Assert.Contains("Stopped", outcome.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Stopping_when_nothing_runs_does_nothing() => new WindowsWingetStore(new WaitingRunner(), Cmd).CancelRunning();

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool OpenProcessToken(IntPtr process, uint access, out IntPtr token);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool GetTokenInformation(IntPtr token, int information, out int value, int length, out int returned);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    private static bool IsElevated(Process process)
    {
        if (!OpenProcessToken(process.Handle, 0x0008, out var token)) throw new InvalidOperationException("No access to the token.");
        try
        {
            Assert.True(GetTokenInformation(token, 20, out var elevated, sizeof(int), out _));
            return elevated != 0;
        }
        finally { CloseHandle(token); }
    }

    [Fact]
    public void A_program_started_through_the_shell_token_has_the_shell_s_elevation()
    {
        // Needs a signed-in desktop. A build server has no shell window, and then nothing can be launched this way.
        if (!UnelevatedLauncher.IsAvailable) return;
        var explorers = Process.GetProcessesByName("explorer");
        if (explorers.Length == 0) return;

        var id = UnelevatedLauncher.Start(Cmd, "/c ping -n 20 127.0.0.1");
        Assert.NotEqual(0, id);
        using var started = Process.GetProcessById(id);
        try
        {
            // Normally the shell is not elevated, so this is the point: Clarion's own elevation is not handed on. On a PC where the shell
            // itself is elevated, such as the built-in Administrator account, the program is elevated the same way.
            var shellElevated = explorers.Select(e => { try { return IsElevated(e); } catch (InvalidOperationException) { return (bool?)null; } }).FirstOrDefault(v => v is not null);
            if (shellElevated is not null) Assert.Equal(shellElevated, IsElevated(started));
        }
        finally
        {
            started.Kill(entireProcessTree: true);
            foreach (var e in explorers) e.Dispose();
        }
    }
}