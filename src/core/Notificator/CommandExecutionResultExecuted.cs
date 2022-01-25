using Lio.Core.Contexts;
using Lio.Core.ExecutionResult;
using MediatR;

namespace Lio.Core.Notificator;

public class CommandExecutionResultExecuted : INotification
{
    public CommandExecutionResultExecuted(ICommandExecutionResult result, CommandExecutionContext commandExecutionContext)
    {
        Result = result;
        CommandExecutionContext = commandExecutionContext;
    }

    public ICommandExecutionResult Result { get; }
    public CommandExecutionContext CommandExecutionContext { get; }
}