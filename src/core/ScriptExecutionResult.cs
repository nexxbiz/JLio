using Lio.Core.Runner;

namespace Lio.Core
{
    public class ScriptExecutionResult
    {
        public ScriptExecutionResult(bool executed, ScriptInstance scriptInstance)
        {
            Executed = executed;
            ScriptInstance = scriptInstance;
        }

        public bool Executed { get; }
        public ScriptInstance ScriptInstance { get; }
    }
}