using Lio.Core.Models;

namespace Lio.Core.Contracts
{
    public interface ISpecificFetcher
    {
        FetchedItems GetItemsForPath(string selectionPath, object data);
    }
}