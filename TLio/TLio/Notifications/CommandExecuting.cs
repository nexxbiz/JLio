using MediatR;
using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Notifications
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