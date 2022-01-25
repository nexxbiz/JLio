using Lio.Core.Contexts;
using MediatR;

namespace Lio.Core.Notificator
{
    public class ScriptExecuting : INotification
    {
        public ScriptExecuting(ScriptExecutionContext scriptExecutionContext)
        {
        }

        public ScriptExecutionContext ScriptExecutionContext { get; }
    }
}