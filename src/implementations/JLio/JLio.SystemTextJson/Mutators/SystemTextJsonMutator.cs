using Lio.Core;
using Lio.Core.Contracts;
using Lio.Core.Models;

namespace JLio.SystemTextJson.Mutators;

public class SystemTextJsonMutator : ISpecificMutator
{
    public void AddValueToArray(FetchedItem item, IFunctionSupportedValue value)
    {
    }

    public FetchedItems GetItemsForPath(string selectionPath)
    {
        return new FetchedItems();
    }

    public void AddValueToObject(FetchedItem item, IFunctionSupportedValue value)
    {
    }

    public FetchedItems GetTargetItems(string path)
    {
        return new FetchedItems();
    }
}