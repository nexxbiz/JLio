using System.Collections.Generic;
using TLio.Contracts;

namespace TLio.Contexts
{
    public class CommandExecutionContext
    {
        public ScriptExecutionContext ScriptExecutionContext { get; }
        
        /// <summary>
        /// Current executing command
        /// </summary>
        public ICommand Command { get;}
        
        public IReadOnlyDictionary<string, object> Input { get; }

        public CommandExecutionContext(ScriptExecutionContext scriptExecutionContext, ICommand command, IReadOnlyDictionary<string, object> input)
        {
            Input = input;
            Command = command;
            ScriptExecutionContext = scriptExecutionContext;
        }
    }
}