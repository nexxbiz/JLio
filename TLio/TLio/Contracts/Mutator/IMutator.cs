using TLio.Implementations;
using TLio.Services.DataFetcher;

namespace TLio.Contracts.Mutator
{
    public interface IMutator
    {
        void AddValueToObject(FetchedItem item, IValue value);
        void AddValueToArray(FetchedItem item, IValue value);
    }
}