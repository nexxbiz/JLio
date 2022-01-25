using Lio.Core.Contexts;
using Lio.Core.Contracts;
using MediatR;

namespace Lio.Core.Notificator
{
    public class CommandExecuting : INotification
    {
        public CommandExecuting(CommandExecutionContext executionContext, ICommand command)
        {
            Command = command;
            CommandExecutionContext = executionContext;
        }

        public ICommand Command { get; }
        public CommandExecutionContext CommandExecutionContext { get; }
    }
}