using MediatR;

namespace JLio2.Core.Notificator;

public class CommandExecuting : INotification
{
    public ExecutionContext ExecutionContext { get; }
    public ICommand Command { get; }

    public CommandExecuting(ExecutionContext executionContext, ICommand command)
    {
        Command = command;
        ExecutionContext = executionContext;
    }
}