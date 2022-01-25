using System.Collections.Generic;
using NodaTime;

namespace Lio.Core.Runner
{
    public class ScriptInstance
    {
        public List<CommandScriptInstance> Commands { get; set; }
        
        public string CurrentCommand { get; set; }
        public ScriptStatus ScriptStatus { get; set; }

    }
    
    public class CommandScriptInstance
    {
        public CommandStatus CommandStatus { get; set; }
        public Instant StartedAt { get; set; }
        public Instant FinishedAt { get; set; }
    }

    public enum CommandStatus
    {
        Faulted,
        Succeeded,
        Error
    }
}