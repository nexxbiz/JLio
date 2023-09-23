using MediatR;
using TLio.Contexts;

namespace TLio.Notifications
{
    public class ScriptExecuting  : INotification
    {
        public ScriptExecuting(LibraryExecutionContext scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
        }

        public LibraryExecutionContext ScriptExecutionContext { get; }
    }
}