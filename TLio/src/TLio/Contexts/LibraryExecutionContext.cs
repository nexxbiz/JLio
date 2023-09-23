using TLio.Contracts;
using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;

namespace TLio.Contexts
{
    public class LibraryExecutionContext : ILibraryExecutionContext
    {

        public LibraryExecutionContext(IDataFetcher dataFetcher, IMutator mutator)
        {
            DataFetcher = dataFetcher;
            Mutator = mutator;
        }
        public IDataFetcher DataFetcher { get; }

        public IMutator Mutator { get; }

        public ICollection<ScriptExecutionLog> ExecutionLog => new List<ScriptExecutionLog>();

        internal void WriteLog(ScriptExecutionLog scriptExecutionLog)
        {
            throw new NotImplementedException();
        }
    }
}