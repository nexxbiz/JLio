using MediatR;
using TLio.Contexts;

namespace TLio.Notifications
{
    public class CommandExecutionFailed<T> : INotification
    {
        public CommandExecutionFailed(Exception exception, CommandExecutionContext<T> commandExecutionContext)
        {
            Exception = exception;
            CommandExecutionContext = commandExecutionContext;
        }

        public Exception Exception { get; }

        public CommandExecutionContext<T> CommandExecutionContext { get; }
    }
}