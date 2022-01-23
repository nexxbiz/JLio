using MediatR;

namespace Lio.Core.Notificator
{
    public class ScriptExecuting : INotification
    {
        public ScriptExecuting(ExecutionContext executionContext)
        {
        }

        public ExecutionContext ExecutionContext { get; }
    }
}