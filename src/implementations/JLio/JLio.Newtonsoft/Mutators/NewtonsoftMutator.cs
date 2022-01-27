using Lio.Core;
using Lio.Core.Contracts;
using Lio.Core.Models;

namespace JLio.Newtonsoft.Mutators;

public class NewtonsoftMutator : ISpecificMutator
{
    public FetchedItems GetItemsForPath(string selectionPath)
    {
        throw new NotImplementedException();
    }

    public void AddValueToObject(FetchedItem item, IFunctionSupportedValue value)
    {
        throw new NotImplementedException();
    }

    public void AddValueToArray(FetchedItem item, IFunctionSupportedValue value)
    {
        throw new NotImplementedException();
    }

    public FetchedItems GetTargetItems(string path)
    {
        throw new NotImplementedException();
    }
}