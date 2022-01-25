using Lio.Core.Contexts;

namespace Lio.Core.ExecutionResult
{
    public class SuccessCommandResult : CommandExecutionResult
    {
        public object? Input { get; }

        public SuccessCommandResult(object? input = null)
        {
            Input = input;
        }
        
        public override void Execute(CommandExecutionContext commandExecutionContext)
        {
            commandExecutionContext.ScriptExecutionContext.Output = Input;
        }
    }
}