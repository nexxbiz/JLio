using Lio.Core.Contexts;
using Lio.Core.Contracts;
using MediatR;

namespace Lio.Core.Notificator
{
    public class CommandExecuted : INotification
    {
        public CommandExecuted(ScriptExecutionContext scriptExecutionContext, ICommand command)
        {
            ScriptExecutionContext = scriptExecutionContext;
            Command = command;
        }

        public ICommand Command { get; }
        public ScriptExecutionContext ScriptExecutionContext { get; }
    }
}