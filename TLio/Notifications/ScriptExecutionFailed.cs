using MediatR;
using TLio.Contexts;

namespace TLio.Notifications
{
    public class ScriptExecutionFailed : INotification
    {
        public ScriptExecutionFailed(Exception exception, ScriptExecutionContext scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
            Exception = exception;
        }

        public ScriptExecutionContext ScriptExecutionContext { get; }
        public Exception Exception { get; }
    }
}