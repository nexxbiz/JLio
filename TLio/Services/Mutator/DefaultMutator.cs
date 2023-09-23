using System.Text.Json.Nodes;
using TLio.Contracts.Mutator;
using TLio.Implementations;
using TLio.Services.DataFetcher;

namespace TLio.Services.Mutator
{
    public class DefaultMutator : IMutator
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