using System.Threading;
using System.Threading.Tasks;
using Lio.Core.Contexts;
using Lio.Core.Notificator;
using MediatR;

namespace Lio.Core.Logs;

public class CommandExecutionLogWriter : INotificationHandler<CommandExecuting>, INotificationHandler<CommandExecuted>,
    INotificationHandler<CommandExecutionFailed>, INotificationHandler<CommandExecutionResultExecuted>
{
    public Task Handle(CommandExecuted notification, CancellationToken cancellationToken)
    {
        WriteEntry("Command executed", notification.CommandExecutionContext);
        return Task.CompletedTask;
    }

    public Task Handle(CommandExecuting notification, CancellationToken cancellationToken)
    {
        WriteEntry($"Executing with data {notification.CommandExecutionContext.Input}",
            notification.CommandExecutionContext);
        return Task.CompletedTask;
    }

    public Task Handle(CommandExecutionFailed notification, CancellationToken cancellationToken)
    {
        WriteEntry($"Command execution failed: {notification.Exception.Message}", notification.CommandExecutionContext);
        return Task.CompletedTask;
    }

    public Task Handle(CommandExecutionResultExecuted notification, CancellationToken cancellationToken)
    {
        var commandExecutionContext = notification.CommandExecutionContext;

        commandExecutionContext.ScriptExecutionContext.Output = commandExecutionContext.Input;
        return Task.CompletedTask;
    }

    private void WriteEntry(string? message, CommandExecutionContext commandExecutionContext)
    {
        commandExecutionContext.AddEntry(message);
    }
}

public class ScriptExecutionLogWriter : INotificationHandler<ScriptExecuting>, INotificationHandler<ScriptExecuted>
{
    public Task Handle(ScriptExecuted notification, CancellationToken cancellationToken)
    {
        WriteEntry("Script executed", notification.ScriptExecutionContext);
        return Task.CompletedTask;
    }

    public Task Handle(ScriptExecuting notification, CancellationToken cancellationToken)
    {
        WriteEntry("Script executing", notification.ScriptExecutionContext);
        return Task.CompletedTask;
    }

    private void WriteEntry(string? message, ScriptExecutionContext scriptExecutionContext)
    {
        scriptExecutionContext.AddEntry(message);
    }
}