using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TLio.Contracts;
using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;
using TLio.Services;

namespace TLio.Contexts
{
    public class ScriptExecutionContext
    {
        public readonly IServiceProvider serviceProvider;
        public ScriptExecutionContext(IServiceProvider serviceProvider,
            Script script,
            IReadOnlyDictionary<string, object> input)
        {
            this.serviceProvider = serviceProvider;
            Script = script;
            Input = input;
            Output = input;
            DataFetcher = GetDataFetcher();
            Mutator = GetMutator();

            //should we fail here if the fetcher or the mutator are not found? because they are readonly so they cannot be set later on
        }
        
        public IReadOnlyDictionary<string, object> Input { get; }
        public IReadOnlyDictionary<string, object> Output { get; private set; }

        public ICollection<CommandExecutionContext> CommandExecutionContexts { get; set; } = new List<CommandExecutionContext>();

        public ICollection<ScriptExecutionLog> ExecutionLog { get; } = new List<ScriptExecutionLog>();
        
        public Script Script { get; }

        public IDataFetcher DataFetcher { get; }
        
        public IMutator Mutator { get; }
        
        public void UpdateOutput(IReadOnlyDictionary<string, object> value)
        {
            Output = value;
        }
        
        private T GetRequiredService<T>() where T : notnull => serviceProvider.GetRequiredService<T>();

        public void WriteLog(ScriptExecutionLog log)
        {
            ExecutionLog.Add(log);
        }

        public IDataFetcher GetDataFetcher()
        {
            var registry = serviceProvider.GetRequiredService<IDataFetcherRegistry>();
            return registry.GetFetcher(Script.FetcherName);
        }
        
        public IMutator GetMutator()
        {
            var registry = serviceProvider.GetRequiredService<IMutatorRegistry>();
            return registry.GetMutator(Script.MutatorName);
        }
    }
}