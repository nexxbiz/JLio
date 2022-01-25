using Lio.Core.Contexts;

namespace Lio.Core.ExecutionResult
{
    public abstract class CommandExecutionResult : ICommandExecutionResult
    {
        public virtual void Execute(CommandExecutionContext commandExecutionContext)
        {
            
        }
    }
}