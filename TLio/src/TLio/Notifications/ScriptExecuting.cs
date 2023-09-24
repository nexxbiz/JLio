using MediatR;
using TLio.Contracts;

namespace TLio.Notifications
{
    public class ScriptExecuting<T>  : INotification
    {
        public ScriptExecuting(ILibraryExecutionContext<T> scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
        }

        public ILibraryExecutionContext<T> ScriptExecutionContext { get; }
    }
}