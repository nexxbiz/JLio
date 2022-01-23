namespace Lio.Core
{
    public class ScriptExecutionResult
    {
        public ScriptExecutionResult(bool executed)
        {
            Executed = executed;
        }

        public bool Executed { get; }
    }
}