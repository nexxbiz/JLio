using TLio.Contracts.DataFetcher;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.SystemTextJson.Contexts
{
    internal class DataFetcher : IDataFetcher
    {
        public object? GetExecutionInput(IReadOnlyDictionary<string, object> input)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object?> GetExecutionResult(object? result)
        {
            throw new NotImplementedException();
        }

        public FetchedItems GetItemsForParentPath(string path, object? input)
        {
            throw new NotImplementedException();
        }

        public FetchedItems GetItemsForPath(string path, object? contextInput)
        {
            throw new NotImplementedException();
        }
    }
}