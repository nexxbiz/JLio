using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;

namespace TLio.Contracts
{
    public interface ILibraryExecutionContext<T>
    {
        IDataFetcher<T> DataFetcher { get; }
        IMutator<T> Mutator { get; }
        ICollection<ScriptExecutionLog> ExecutionLog { get; }
        void WriteLog(ScriptExecutionLog scriptExecutionLog);
    }
}