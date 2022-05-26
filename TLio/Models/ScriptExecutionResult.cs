namespace TLio.Models
{
    public class ScriptExecutionResult
    {
        public IDictionary<string, IDictionary<string, object?>> CommandOutput =
            new Dictionary<string, IDictionary<string, object?>>();
        
        public ScriptExecutionType ScriptExecutionType { get; set; }

        public List<ScriptExecutionLog> ExecutionLogs { get; set; } = new List<ScriptExecutionLog>();

        public IReadOnlyDictionary<string, object> Output = new Dictionary<string, object>();
    }

    public enum ScriptExecutionType
    {
        Completed,
        CompletedWithErrors,
        Failed
    }
}