using MediatR;
using TLio.Models;

namespace TLio.Notifications.Implementations
{
    public class ScriptExecutedNotificator : INotificationHandler<ScriptExecuted>
    {
        public Task Handle(ScriptExecuted notification, CancellationToken cancellationToken)
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