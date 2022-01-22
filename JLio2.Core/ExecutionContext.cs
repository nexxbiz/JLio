using Microsoft.Extensions.DependencyInjection;

namespace JLio2.Core;

public class ExecutionContext
{
    public object? Input { get; }
    public ScriptExecutionLog ScriptExecutionLog { get; }
    
    public ISpecificMutator SpecificMutator { get; }
    
    public IDictionary<string, object?> JournalData { get; private set; } = new Dictionary<string, object?>();

    public ExecutionContext(IServiceProvider serviceProvider, object input)
    {
        Input = input;
        ScriptExecutionLog = ActivatorUtilities.CreateInstance<ScriptExecutionLog>(serviceProvider);
    }
    
    public TType GetInput<TType>()
    {
        return (TType) Input;
    }

    public void AddEntry(string commandName, string message)
    {
        ScriptExecutionLog.AddEntry(commandName, message);
    }
}

public enum TargetTypes
{
    Array,
    Object,
    String,
    Integer
}
