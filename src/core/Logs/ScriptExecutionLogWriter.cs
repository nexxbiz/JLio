using System.Threading;
using System.Threading.Tasks;
using Lio.Core.Contexts;
using Lio.Core.Notificator;
using MediatR;

namespace Lio.Core.Logs;

public class ScriptExecutionLogWriter : INotificationHandler<CommandExecuting>, INotificationHandler<CommandExecuted>, INotificationHandler<CommandExecutionFailed>, INotificationHandler<CommandExecutionResultExecuted>
{
    public Task Handle(CommandExecuting notification, CancellationToken cancellationToken)
    {
        WriteEntry($"Executing with data {notification.CommandExecutionContext.Input}",
            notification.CommandExecutionContext);
        return Task.CompletedTask;
    }

    public Task Handle(CommandExecuted notification, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    public Task Handle(CommandExecutionFailed notification, CancellationToken cancellationToken)
    {
        WriteEntry($"Command Execution Failed {notification.Exception.Message}", notification.CommandExecutionContext);
        return Task.CompletedTask;
    }

    public Task Handle(CommandExecutionResultExecuted notification, CancellationToken cancellationToken)
    {
        var commandExecutionContext = notification.CommandExecutionContext;

        commandExecutionContext.ScriptExecutionContext.Output = commandExecutionContext.Input;
        return Task.CompletedTask;
    }

    private void WriteEntry(string? message, CommandExecutionContext commandExecutionContext) => commandExecutionContext.AddEntry(message);

}