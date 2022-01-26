using Lio.Core;
using Lio.Core.Contracts;
using Lio.Core.Models;

namespace JLio.Newtonsoft.Mutators;

public class NewtonsoftMutator : ISpecificMutator
{
    private readonly ISpecificFetcher fetcher;

    public NewtonsoftMutator(ISpecificFetcher fetcher)
    {
        this.fetcher = fetcher;
    }

    public FetchedItems GetTargetItems(string path)
    {
        throw new NotImplementedException();
    }

    public void AddItemToArray(FetchedItem item, IFunctionSupportedValue value)
    {
        throw new NotImplementedException();
    }

    public FetchedItems GetItemsForSelectionPath(string selectionPath)
    {
        throw new NotImplementedException();
    }

    public void AddItemToObject(FetchedItem item, IFunctionSupportedValue value)
    {
        throw new NotImplementedException();
    }
}