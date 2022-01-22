namespace JLio2.Core;

public interface ICommand
{
    Type Type { get; }
    string Name { get; } 
    IDictionary<string, object?> Data { get; set; }
    bool CanExecute(ExecutionContext executionContext);
    IExecutionResult Execute(ExecutionContext executionContext);
}
