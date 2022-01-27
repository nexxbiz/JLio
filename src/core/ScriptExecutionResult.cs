using System.Collections.Generic;
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
        public List<ExecutionLogRecord> ExecutionLog { get; set; }
        public ScriptInstance ScriptInstance { get; }
    }
}