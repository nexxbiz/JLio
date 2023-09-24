using MediatR;
using TLio.Models;

namespace TLio.Notifications.Implementations
{
    public class ScriptNotificator<T> : INotificationHandler<ScriptExecuting<T>>
    {
        public Task Handle(ScriptExecuting<T> notification, CancellationToken cancellationToken)
        {
            var scriptContext = notification.ScriptExecutionContext;
            
            notification.ScriptExecutionContext.WriteLog(new ScriptExecutionLog()
            {
                Message = "Script started to execute. Mutator " + scriptContext.Mutator?.GetType().Name +
                " . Fetcher " + scriptContext.DataFetcher.GetType().Name,
                Timestamp = DateTimeOffset.UtcNow
            });
            return Task.CompletedTask;
        }
    }
}