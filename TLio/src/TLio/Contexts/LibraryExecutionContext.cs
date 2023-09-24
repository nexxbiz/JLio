using TLio.Contracts;
using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;

namespace TLio.Contexts
{
    public class LibraryExecutionContext<T> : ILibraryExecutionContext<T>
    {

        public LibraryExecutionContext(IDataFetcher<T> dataFetcher, IMutator<T> mutator)
        {
            DataFetcher = dataFetcher;
            Mutator = mutator;
        }
        public IDataFetcher<T> DataFetcher { get; }

        public IMutator<T> Mutator { get; }

        public ICollection<ScriptExecutionLog> ExecutionLog => new List<ScriptExecutionLog>();

        internal void WriteLog(ScriptExecutionLog scriptExecutionLog)
        {
            throw new NotImplementedException();
        }

        void ILibraryExecutionContext<T>.WriteLog(ScriptExecutionLog scriptExecutionLog)
        {
            throw new NotImplementedException();
        }
    }
}