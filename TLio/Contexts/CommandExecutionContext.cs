namespace TLio.Contexts
{
    public class CommandExecutionContext
    {
        public ScriptExecutionContext ScriptExecutionContext { get; }
        
        public IReadOnlyDictionary<string, object> Input { get; }

        public CommandExecutionContext(ScriptExecutionContext scriptExecutionContext, IReadOnlyDictionary<string, object> input)
        {
            Input = input;
            ScriptExecutionContext = scriptExecutionContext;
        }
    }
}