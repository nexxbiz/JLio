using MediatR;
using TLio.Contexts;

namespace TLio.Notifications
{
    public class ScriptExecutionFailed : INotification
    {
        public ScriptExecutionFailed(Exception exception, LibraryExecutionContext scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
            Exception = exception;
        }

        public LibraryExecutionContext ScriptExecutionContext { get; }
        public Exception Exception { get; }
    }
}