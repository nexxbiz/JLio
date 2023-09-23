using TLio.Contracts;
using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;

namespace TLio.Implementations.SystemTextJson.Contexts
{
    internal class ExecutionContext : ILibraryExecutionContext
    {
        public IDataFetcher DataFetcher => new DataFetcher();

        public IMutator Mutator => new Mutator();

        public ICollection<ScriptExecutionLog> ExecutionLog => new List<ScriptExecutionLog>();
    }
}
