using TLio.Implementations;
using TLio.Services.DataFetcher;

namespace TLio.Contracts.Mutator
{
    public interface IMutator
    {
        void AddValueToObject(FetchedItem item, object value, string propertyName);
        void AddValueToArray(FetchedItem item, object value, string? propertyName = default);
    }
}