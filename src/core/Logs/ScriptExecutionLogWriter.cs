using System.Threading;
using System.Threading.Tasks;
using Lio.Core.Contexts;
using Lio.Core.Notificator;
using MediatR;

namespace Lio.Core.Logs;

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