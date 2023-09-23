using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;

namespace TLio.Contracts
{
    public interface ILibraryExecutionContext
    {
        IDataFetcher DataFetcher { get; }
        IMutator Mutator { get; }
        ICollection<ScriptExecutionLog> ExecutionLog { get; } 
    }
}