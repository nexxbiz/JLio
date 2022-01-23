namespace Lio.Core
{
    public class CommandExecutionResult : ExecutionResult
    {
        private ExecutionContext ExecutionContext { get; }
        public object? Input { get; }
    }
}