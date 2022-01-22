using System.ComponentModel;

namespace JLio2.Core;

public interface IExecutionResult
{
    
}

public class CommandExecutionResult : ExecutionResult
{
    public object? Input { get; }

    private ExecutionContext ExecutionContext { get; }
}

public class ExecutionResult : IExecutionResult
{
    public ScriptExecutionLog ScriptExecutionLog;
}