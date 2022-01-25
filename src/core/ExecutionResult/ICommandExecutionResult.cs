using Lio.Core.Contexts;

namespace Lio.Core.ExecutionResult
{
    public interface ICommandExecutionResult
    {
        public void Execute(CommandExecutionContext commandExecutionContext);
    }
}