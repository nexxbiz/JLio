using System.Collections.Generic;
using MediatR;
using TLio.Contexts;
using TLio.Contracts;

namespace TLio.Notifications
{
    public class CommandExecuted : INotification
    {
        public CommandExecuted(CommandExecutionContext commandExecutionContext, ICommand command)
        {
            CommandExecutionContext = commandExecutionContext;
            Command = command;
        }

        public ICommand Command { get; }
        
        public Dictionary<string, object> Output { get; }
        public CommandExecutionContext CommandExecutionContext { get; }
    }
}