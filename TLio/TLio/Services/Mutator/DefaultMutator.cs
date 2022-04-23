using TLio.Contracts.Mutator;
using TLio.Implementations;
using TLio.Services.DataFetcher;

namespace TLio.Services.Mutator
{
    public class DefaultMutator : IMutator
    {
        public void AddValueToObject(FetchedItem item, IValue value)
        {
            throw new System.NotImplementedException();
        }

        public void AddValueToArray(FetchedItem item, IValue value)
        {
            throw new System.NotImplementedException();
        }
    }
}