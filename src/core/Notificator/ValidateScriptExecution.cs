using Lio.Core.Contexts;
using Lio.Core.Models;
using MediatR;

namespace Lio.Core.Notificator
{
    public class ValidateScriptExecution : INotification
    {
        public ValidateScriptExecution(ScriptExecutionContext scriptExecutionContext, ScriptDefinition scriptDefinition,
            ScriptInput input)
        {
            ScriptExecutionContext = scriptExecutionContext;
            ScriptDefinition = scriptDefinition;
            Input = input;
        }

        public bool CanExecuteScript { get; private set; } = true;
        public ScriptExecutionContext ScriptExecutionContext { get; }
        public ScriptInput Input { get; }
        public ScriptDefinition ScriptDefinition { get; }

        public void PreventScriptExecution()
        {
            CanExecuteScript = false;
        }
    }
}