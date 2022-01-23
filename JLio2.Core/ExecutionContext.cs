using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Lio.Core
{
    public class ExecutionContext
    {
        public ExecutionContext(IServiceProvider serviceProvider, object input)
        {
            Input = input;
            ScriptExecutionLog = ActivatorUtilities.CreateInstance<ScriptExecutionLog>(serviceProvider);
        }

        public object? Input { get; }

        public IDictionary<string, object?> JournalData { get; } = new Dictionary<string, object?>();
        public ScriptExecutionLog ScriptExecutionLog { get; }

        public ISpecificMutator SpecificMutator { get; }

        public TType GetInput<TType>()
        {
            return (TType)Input;
        }

        public void AddEntry(string commandName, string message)
        {
            ScriptExecutionLog.AddEntry(commandName, message);
        }
    }
}