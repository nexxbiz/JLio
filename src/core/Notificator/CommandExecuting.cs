using MediatR;

namespace Lio.Core.Notificator
{
    public class CommandExecuting : INotification
    {
        public CommandExecuting(ExecutionContext executionContext, ICommand command)
        {
            Command = command;
            ExecutionContext = executionContext;
        }

        public ICommand Command { get; }
        public ExecutionContext ExecutionContext { get; }
    }
}