using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TLio.Models;

namespace TLio.Notifications.Implementations
{
    public class CommandExecutingNotificator : INotificationHandler<CommandExecuting>
    {
        public Task Handle(CommandExecuting notification, CancellationToken cancellationToken)
        {
            var executionContext = notification.CommandExecutionContext;
            
            executionContext.ScriptExecutionContext.ExecutionLog.Add(new ScriptExecutionLog
            {
                CommandName = executionContext.Command.Name,
                Message = "Executing command",
                Payload = executionContext.Input,
                Timestamp = DateTimeOffset.UtcNow
            });
            
            executionContext.ScriptExecutionContext.CommandExecutionContexts.Add(executionContext);

            return Task.CompletedTask;
        }
    }
}