using TLio.Services.DataFetcher;

namespace TLio.Contracts.Mutator
{
    public interface IMutator<T>
    {
        void AddValueToObject(FetchedItem<T> item, T value, string propertyName);
        void AddValueToArray(FetchedItem<T> item, T value, string? propertyName = default);
    }
}