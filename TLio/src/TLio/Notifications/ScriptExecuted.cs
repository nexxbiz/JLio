using MediatR;
using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Notifications
{
    public class ScriptExecuted<T> : INotification
    {
        public ScriptExecuted(ILibraryExecutionContext<T> scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
        }

        public ILibraryExecutionContext<T> ScriptExecutionContext { get; }
    }
}