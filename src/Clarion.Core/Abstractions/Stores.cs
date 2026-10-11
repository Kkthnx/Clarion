using Clarion.Core.Model;

namespace Clarion.Core.Abstractions;

public interface IServiceStore
{
    /// <summary>Current start type, or null when the service is missing or is a driver or boot service.</summary>
    ServiceStartType? GetStartType(string name);
    void SetStartType(string name, ServiceStartType startType);
}

public interface ITaskStore
{
    /// <summary>Whether the task is enabled, or null when it does not exist.</summary>
    bool? GetEnabled(string path);
    void SetEnabled(string path, bool enabled);
}

public sealed record ProcessResult(int ExitCode, string Output, string Error);

public interface IProcessRunner
{
    ProcessResult Run(string fileName, string arguments, TimeSpan timeout);

    /// <summary>
    /// Runs the program and stops it, with the programs it started, when the token is cancelled. Exit code -2 means it was stopped that way,
    /// and -1 means it ran out of time. Every runner has to say how it stops a program, so a runner that cannot is a visible choice.
    /// </summary>
    ProcessResult Run(string fileName, string arguments, TimeSpan timeout, CancellationToken cancel);
}
