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
            DataFetcher = GetDataFetcher() ?? throw new Exception($"Could not find fetcher with name {Script.FetcherName}" );
            Mutator = GetMutator() ?? throw new Exception($"Could not find mutator with name {Script.MutatorName}" );
            Input = DataFetcher.GetExecutionInput(input);
            Output = Input;
        }
       

        public object? Input { get; }
        public object? Output { get; private set; }

        public ICollection<CommandExecutionContext> CommandExecutionContexts { get; set; } = new List<CommandExecutionContext>();

        public ICollection<ScriptExecutionLog> ExecutionLog { get; } = new List<ScriptExecutionLog>();
        
        public Script Script { get; }

        public IDataFetcher DataFetcher { get; }
        
        public IMutator Mutator { get; }
        
        public void UpdateOutput(object? value)
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