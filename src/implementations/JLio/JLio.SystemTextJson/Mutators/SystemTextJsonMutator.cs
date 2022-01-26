using Lio.Core;
using Lio.Core.Contracts;
using Lio.Core.Models;

namespace JLio.SystemTextJson.Mutators;

public class SystemTextJsonMutator : ISpecificMutator
{
    private readonly ISpecificFetcher fetcher;

    public SystemTextJsonMutator(ISpecificFetcher fetcher)
    {
        this.fetcher = fetcher;
    }

    public FetchedItems GetTargetItems(string path)
    {
        return new FetchedItems();
    }

    public void AddItemToArray(FetchedItem item, IFunctionSupportedValue value)
    {
    }

    public FetchedItems GetItemsForSelectionPath(string selectionPath)
    {
        return new FetchedItems();
    }

    public void AddItemToObject(FetchedItem item, IFunctionSupportedValue value)
    {
    }
}