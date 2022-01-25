using System;
using Lio.Core.Contexts;

namespace Lio.Core.ExecutionResult
{
    public class FaultCommandResult : CommandExecutionResult
    {
        public object? Input { get; }
        
        public FaultCommandResult(Exception ex, object? input = null)
        {
            Input = input;
            Exception = ex;
        }

        public FaultCommandResult(string message, object? input = null)
        {
            Message = message;
            Input = input;
        }

        public Exception Exception { get; } = default!;
        public string Message { get; } = default!;
        
        public override void Execute(CommandExecutionContext commandExecutionContext)
        {
            if(Exception != null!)
                commandExecutionContext.ScriptExecutionContext.Fault(Exception, Exception.Message, commandExecutionContext.CommandName, commandExecutionContext.Input );
            else 
                commandExecutionContext.ScriptExecutionContext.Fault(null, Message, commandExecutionContext.CommandName, commandExecutionContext.Input );
            
            commandExecutionContext.ScriptExecutionContext.Output = Input;
        }
    }
}