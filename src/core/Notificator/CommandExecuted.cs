using Lio.Core.Contexts;
using Lio.Core.Contracts;
using MediatR;

namespace Lio.Core.Notificator
{
    public class CommandExecuted : INotification
    {
        public CommandExecuted(CommandExecutionContext commandExecutionContext, ICommand command)
        {
            CommandExecutionContext = commandExecutionContext;
            Command = command;
        }

        public ICommand Command { get; }
        public CommandExecutionContext CommandExecutionContext { get; }
    }
}