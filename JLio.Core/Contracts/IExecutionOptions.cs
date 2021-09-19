using JLio.Core.Models.Logging;

namespace JLio.Core.Contracts
{
    public interface IExecutionOptions
    {
        IItemsFetcher ItemsFetcher { get; set; }
        IExecutionLogger Logger { get; set; }
        void LogWarning(string group, string message);

        void LogError(string group, string message);

        void LogInfo(string group, string message);

        LogEntries GetLogEntries();
    }
}