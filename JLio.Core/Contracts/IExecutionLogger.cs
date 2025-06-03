using JLio.Core.Models.Logging;
using Microsoft.Extensions.Logging;

namespace JLio.Core.Contracts;

public interface IExecutionLogger
{
    LogEntries LogEntries { get; }
    string LogText { get; }
    void Log(LogLevel logLevel, string group, string message);
}