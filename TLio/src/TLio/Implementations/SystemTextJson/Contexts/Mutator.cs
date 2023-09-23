using TLio.Contracts.Mutator;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.SystemTextJson.Contexts
{
    internal class Mutator : IMutator
    {
        public void AddValueToArray(FetchedItem item, object value, string? propertyName = null)
        {
            throw new NotImplementedException();
        }

        public void AddValueToObject(FetchedItem item, object value, string propertyName)
        {
            throw new NotImplementedException();
        }
    }
}