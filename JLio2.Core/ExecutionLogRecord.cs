using NodaTime;

namespace Lio.Core
{
    public class ExecutionLogRecord
    {
        public ExecutionLogRecord()
        {
        }

        public ExecutionLogRecord(string commandName, Instant timestamp, string? message)
        {
            CommandName = commandName;
            Timestamp = timestamp;
            Message = message;
        }

        public string CommandName { get; set; }
        public string? Message { get; set; }
        public Instant Timestamp { get; set; }
    }
}