using MediatR;
using TLio.Models;

namespace TLio.Notifications.Implementations
{
    public class ScriptExecutedNotificator<T> : INotificationHandler<ScriptExecuted<T>>
    {
        public Task Handle(ScriptExecuted<T> notification, CancellationToken cancellationToken)
        {
            var scriptExecuted = notification.ScriptExecutionContext;
            
            notification.ScriptExecutionContext.ExecutionLog.Add(new ScriptExecutionLog
            {
                Message = "",
                Timestamp = DateTimeOffset.UtcNow
            });

            return Task.CompletedTask;
        }
    }
}