namespace TLio.Models
{
    public class ScriptExecutionResult
    {
        public IDictionary<string, IDictionary<string, object?>> CommandOutput =
            new Dictionary<string, IDictionary<string, object?>>();
        
        public ScriptExecutionType ScriptExecutionType { get; set; }

        public List<ScriptExecutionLog> ExecutionLogs { get; set; } = new List<ScriptExecutionLog>();

        public object? Output = null;
    }

    public enum ScriptExecutionType
    {
        Completed,
        CompletedWithErrors,
        Failed
    }
}