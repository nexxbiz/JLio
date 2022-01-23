using Lio.Core.Models;
using MediatR;

namespace Lio.Core.Notificator
{
    public class ValidateScriptExecution : INotification
    {
        public ValidateScriptExecution(ExecutionContext executionContext, ScriptDefinition scriptDefinition,
            ScriptInput input)
        {
            ExecutionContext = executionContext;
            ScriptDefinition = scriptDefinition;
            Input = input;
        }

        public bool CanExecuteScript { get; private set; } = true;
        public ExecutionContext ExecutionContext { get; }
        public ScriptInput Input { get; }
        public ScriptDefinition ScriptDefinition { get; }

        public void PreventScriptExecution()
        {
            CanExecuteScript = false;
        }
    }
}