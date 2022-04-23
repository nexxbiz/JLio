using System;
using MediatR;
using TLio.Contexts;

namespace TLio.Notifications
{
    public class CommandExecutionFailed : INotification
    {
        public CommandExecutionFailed(Exception exception, CommandExecutionContext commandExecutionContext)
        {
            Exception = exception;
            CommandExecutionContext = commandExecutionContext;
        }

        public Exception Exception { get; }

        public CommandExecutionContext CommandExecutionContext { get; }
    }
}