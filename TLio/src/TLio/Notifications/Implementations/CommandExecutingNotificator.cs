using MediatR;
using TLio.Models;

namespace TLio.Notifications.Implementations
{
    //public class CommandExecutingNotificator : INotificationHandler<CommandExecuting>
    //{
    //    public Task Handle(CommandExecuting notification, CancellationToken cancellationToken)
    //    {
    //        var executionContext = notification.CommandExecutionContext;

    //        executionContext.ExecutionContext.WriteLog(new ScriptExecutionLog
    //        {
    //            CommandName = notification.Command.Name,
    //            Message = "Executing command",
    //            Payload = executionContext.Input,
    //            Timestamp = DateTimeOffset.UtcNow
    //        });
            
    //        return Task.CompletedTask;
    //    }
    //}
}