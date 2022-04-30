using MediatR;
using TLio.Contexts;

namespace TLio.Notifications
{
    public class ScriptExecuting  : INotification
    {
        public ScriptExecuting(ScriptExecutionContext scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
        }

        public ScriptExecutionContext ScriptExecutionContext { get; }
    }
}