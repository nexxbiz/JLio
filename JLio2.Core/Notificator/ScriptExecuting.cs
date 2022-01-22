using MediatR;

namespace JLio2.Core.Notificator;

public class ScriptExecuting : INotification
{
    public ScriptExecuting(ExecutionContext executionContext) 
    {
    }
    
    public ExecutionContext ExecutionContext { get; }

}