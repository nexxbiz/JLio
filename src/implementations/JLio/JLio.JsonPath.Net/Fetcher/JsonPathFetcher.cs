using Lio.Core.Contracts;
using Lio.Core.Models;

namespace JLio.JsonPath.Net.Fetcher
{
    public class JsonPathFetcher : ISpecificFetcher
    {
        public FetchedItems GetItemsForPath(string selectionPath, object data)
        {
            return FetchedItems.Failed();
        }
    }
}