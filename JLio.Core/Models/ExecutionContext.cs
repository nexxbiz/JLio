using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models.Logging;
using Microsoft.Extensions.Logging;

namespace JLio.Core.Models;

public class ExecutionContext : IExecutionContext
{
    public IExecutionLogger Logger { get; set; }

    public IItemsFetcher ItemsFetcher { get; set; }

    public void LogWarning(string group, string message)
    {
        Logger?.Log(LogLevel.Warning, group, message);
    }

    public void LogError(string group, string message)
    {
        Logger?.Log(LogLevel.Error, group, message);
    }

    public void LogInfo(string group, string message)
    {
        Logger?.Log(LogLevel.Information, group, message);
    }

    public LogEntries GetLogEntries()
    {
        return Logger == null ? new LogEntries() : Logger.LogEntries;
    }

    public static IExecutionContext CreateDefault()
    {
        return new ExecutionContext
            {ItemsFetcher = new JsonPathItemsFetcher(), Logger = new ExecutionLogger()};
    }
}