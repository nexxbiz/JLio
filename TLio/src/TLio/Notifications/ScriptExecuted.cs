using MediatR;
using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Notifications
{
    public class ScriptExecuted : INotification
    {
        public ScriptExecuted(ILibraryExecutionContext scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
        }

        public ILibraryExecutionContext ScriptExecutionContext { get; }
    }
}