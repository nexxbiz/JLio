using MediatR;
using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Notifications
{
    public class CommandExecuted<T> : INotification
    {
        public CommandExecuted(CommandExecutionContext<T> commandExecutionContext, ICommand command)
        {
            CommandExecutionContext = commandExecutionContext;
            Command = command;
        }

        public ICommand Command { get; }
        
        public Dictionary<string, object> Output { get; }
        public CommandExecutionContext<T> CommandExecutionContext { get; }
    }
}