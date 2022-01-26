using Lio.Core.Contexts;
using MediatR;

namespace Lio.Core.Notificator
{
    public class ScriptExecuting : INotification
    {
        public ScriptExecuting(ScriptExecutionContext scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
        }

        public ScriptExecutionContext ScriptExecutionContext { get; }
    }

    public class ScriptExecuted : INotification
    {
        public ScriptExecuted(ScriptExecutionContext scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
        }

        public ScriptExecutionContext ScriptExecutionContext { get; }
    }
}