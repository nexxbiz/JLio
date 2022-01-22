namespace JLio2.Core;

public abstract class Command : ICommand
{
    public Type Type => GetType();
    public string Name { get; set; }
    public IDictionary<string, object?> Data { get; set; } = new Dictionary<string, object?>();

    public bool CanExecute(ExecutionContext executionContext) => OnCanExecute(executionContext);

    public IExecutionResult Execute(ExecutionContext executionContext) => OnExecute(executionContext);
   
    protected virtual IExecutionResult OnExecute(ExecutionContext context) => OnExecute();

    protected virtual IExecutionResult OnExecute() => new ExecutionResult();
    
    protected virtual bool OnCanExecute(ExecutionContext context) => true;
}