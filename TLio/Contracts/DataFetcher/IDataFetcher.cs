using TLio.Services.DataFetcher;

namespace TLio.Contracts.DataFetcher
{
    public interface IDataFetcher
    {
        FetchedItems GetItemsForPath(string path, object? contextInput);
        object? GetExecutionInput(IReadOnlyDictionary<string, object> input);
        FetchedItems GetItemsForParentPath(string path, object? input);
    }
}