using NodaTime;

namespace JLio2.Core;

public class ScriptExecutionLog
{
    private readonly IClock _clock;
    private readonly ICollection<ExecutionLogRecord> _records = new List<ExecutionLogRecord>();

    public ScriptExecutionLog(IClock clock)
    {
        _clock = clock;
    }
    
    public void AddEntry(string commandName, string? message)
    {
        var timeStamp = _clock.GetCurrentInstant();
        var record = new ExecutionLogRecord(commandName, timeStamp, message);
        _records.Add(record);
    }
    
    public void Flush()
    {
        _records.Clear();
    }

}