using Lio.Core.Contexts;
using Lio.Core.ExecutionResult;

namespace Lio.Core.Contracts
{
    public interface ICommand
    {
        string Name { get; }
        bool CanExecute(CommandExecutionContext executionContext);
        ICommandExecutionResult Execute(CommandExecutionContext executionContext);
    }
}