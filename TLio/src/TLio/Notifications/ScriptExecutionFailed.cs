using MediatR;
using TLio.Contracts;

namespace TLio.Notifications
{
    public class ScriptExecutionFailed<T> : INotification
    {
        public ScriptExecutionFailed(Exception exception, ILibraryExecutionContext<T> scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
            Exception = exception;
        }

        public ILibraryExecutionContext<T> ScriptExecutionContext { get; }
        public Exception Exception { get; }
    }
}