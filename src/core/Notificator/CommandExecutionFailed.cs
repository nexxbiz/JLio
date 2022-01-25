using System;
using Lio.Core.Contexts;
using MediatR;

namespace Lio.Core.Notificator
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