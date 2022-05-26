using Microsoft.Extensions.DependencyInjection;
using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;

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
            DataFetcher = GetDataFetcher() ?? throw new Exception($"Could not find fetcher with name {Script.FetcherName}" );;
            Mutator = GetMutator() ?? throw new Exception($"Could not find mutator with name {Script.MutatorName}" );;
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

        private IDataFetcher GetDataFetcher()
        {
            var registry = serviceProvider.GetRequiredService<IDataFetcherRegistry>();
            return registry.GetFetcher(Script.FetcherName);
        }
        
        private IMutator GetMutator()
        {
            var registry = serviceProvider.GetRequiredService<IMutatorRegistry>();
            return registry.GetMutator(Script.MutatorName);
        }
    }
}