using MediatR;

namespace JLio2.Core.Notificator;

public class CommandExecuted : INotification
{
    public ExecutionContext ExecutionContext { get; }
    public ICommand Command { get; }

    public CommandExecuted(ExecutionContext executionContext, ICommand command)
    {
        ExecutionContext = executionContext;
        Command = command;
    }
}