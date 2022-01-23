using MediatR;

namespace Lio.Core.Notificator
{
    public class CommandExecuted : INotification
    {
        public CommandExecuted(ExecutionContext executionContext, ICommand command)
        {
            ExecutionContext = executionContext;
            Command = command;
        }

        public ICommand Command { get; }
        public ExecutionContext ExecutionContext { get; }
    }
}