using System.Collections.Generic;
using NodaTime;

namespace Lio.Core
{
    public class ScriptExecutionLog
    {
        private readonly IClock _clock;
        private readonly ICollection<ExecutionLogRecord> records = new List<ExecutionLogRecord>();

        public ScriptExecutionLog(IClock clock)
        {
            _clock = clock;
        }

        public void AddEntry(string commandName, string? message)
        {
            var timeStamp = _clock.GetCurrentInstant();
            var record = new ExecutionLogRecord(commandName, timeStamp, message);
            records.Add(record);
        }

        public ICollection<ExecutionLogRecord> GetEntries()
        {
            return records;
        }

        public void Flush()
        {
            records.Clear();
        }
    }
}