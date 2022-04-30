using MediatR;
using TLio.Contexts;

namespace TLio.Notifications
{
    public class ScriptExecuted : INotification
    {
        public ScriptExecuted(ScriptExecutionContext scriptExecutionContext)
        {
            ScriptExecutionContext = scriptExecutionContext;
        }

        public ScriptExecutionContext ScriptExecutionContext { get; }
    }
}