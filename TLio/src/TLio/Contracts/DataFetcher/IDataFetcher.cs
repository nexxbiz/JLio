using TLio.Services.DataFetcher;

namespace TLio.Contracts.DataFetcher
{
    public interface IDataFetcher<T>
    {
        FetchedItems<T> GetItemsForPath(string path, T? contextInput);
        T? GetExecutionInput(IReadOnlyDictionary<string, T> input);
        FetchedItems<T> GetItemsForParentPath(string path, T? input);
        Dictionary<string, T?> GetExecutionResult(T? result);
    }
}