using MediatR;

namespace JLio2.Core.Notificator;

public class CommandExecutionFailed : INotification
{
    public CommandExecutionFailed(Exception exception, ExecutionContext executionContext, string commandName)
    {
        Exception = exception;
        ExecutionContext = executionContext;
    }
    
    public Exception Exception { get; }
    
    public ExecutionContext ExecutionContext { get; }
}