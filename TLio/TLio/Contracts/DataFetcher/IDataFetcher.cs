using System.Collections.Generic;
using TLio.Services;
using TLio.Services.DataFetcher;

namespace TLio.Contracts.DataFetcher
{
    public interface IDataFetcher
    {
        FetchedItems GetItemsForPath(string path, IReadOnlyDictionary<string, object> contextInput);
    }
}