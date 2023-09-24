using MediatR;
using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Notifications
{
    public class CommandExecuting<T> : INotification
    {
        public CommandExecuting(CommandExecutionContext<T> executionContext, ICommand<T> command)
        {
            Command = command;
            CommandExecutionContext = executionContext;
        }

        public ICommand<T> Command { get; }
        public CommandExecutionContext<T> CommandExecutionContext { get; }
    }
}