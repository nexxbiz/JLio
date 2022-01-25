using System;
using System.Collections.Generic;
using Lio.Core.Contracts;

namespace Lio.Core.Contexts
{
    public class CommandExecutionContext
    {
        public string CommandName { get; }
        public object? Input { get; }
        
        public object? Output { get; private set; }

        public ScriptExecutionContext ScriptExecutionContext { get; }
        
        public IDictionary<string, object?> JournalData { get; private set; } = new Dictionary<string, object?>();

        public ScriptExecutionLog ScriptExecutionLog => ScriptExecutionContext.ScriptExecutionLog;
        
        public CommandExecutionContext(object? input, ScriptExecutionContext scriptExecutionContext, string commandName)
        {
            Input = input;
            ScriptExecutionContext = scriptExecutionContext;
            CommandName = commandName;
        }

        public void Fault(Exception exception) =>
            ScriptExecutionContext.Fault(exception, exception.Message, CommandName, Input);


        public void Success() =>
            ScriptExecutionContext.Success(CommandName);

        public void LogOutput(ICommand command, object? value)
        {
            JournalData.Add("Output", value);
        }

        public void AddEntry(string? message)
        {
            ScriptExecutionLog.AddEntry(CommandName, message);
        }

        public void SetOutput(object output)
        {
            Output = output;
        }
    }
}