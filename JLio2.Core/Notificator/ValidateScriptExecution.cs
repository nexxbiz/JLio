using JLio2.Core.Models;
using MediatR;

namespace JLio2.Core.Notificator;

public class ValidateScriptExecution : INotification
{
    public  ExecutionContext ExecutionContext { get; }
    public ScriptDefinition ScriptDefinition { get; }
    public ScriptInput Input { get; }

    public ValidateScriptExecution(ExecutionContext executionContext, ScriptDefinition scriptDefinition, ScriptInput input)
    {
        ExecutionContext = executionContext;
        ScriptDefinition = scriptDefinition;
        Input = input;
    }

    public bool CanExecuteScript { get; private set; } = true;
    public void PreventScriptExecution() => CanExecuteScript = false;
}