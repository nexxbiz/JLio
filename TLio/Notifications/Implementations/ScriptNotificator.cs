using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TLio.Models;

namespace TLio.Notifications.Implementations
{
    public class ScriptNotificator : INotificationHandler<ScriptExecuting>
    {
        public Task Handle(ScriptExecuting notification, CancellationToken cancellationToken)
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