namespace TLio.Contexts
{
    public class CommandExecutionContext
    {
        public ScriptExecutionContext ScriptExecutionContext { get; }
        
        public object? Input { get; }

        public CommandExecutionContext(ScriptExecutionContext scriptExecutionContext, object? input)
        {
            Input = input;
            ScriptExecutionContext = scriptExecutionContext;
        }
    }
}