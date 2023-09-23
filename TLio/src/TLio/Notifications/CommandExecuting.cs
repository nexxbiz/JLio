using MediatR;
using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Notifications
{
    public class CommandExecuting<T> : INotification
    {
        public CommandExecuting(CommandExecutionContext<T> executionContext, ICommand command)
        {
            Command = command;
            CommandExecutionContext = executionContext;
        }

        public ICommand Command { get; }
        public CommandExecutionContext<T> CommandExecutionContext { get; }
    }
}