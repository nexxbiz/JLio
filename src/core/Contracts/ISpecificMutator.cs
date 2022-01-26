using Lio.Core.Models;

namespace Lio.Core.Contracts
{
    public interface ISpecificMutator
    {
        FetchedItems GetTargetItems(string path);
        void AddItemToArray(FetchedItem item, IFunctionSupportedValue value);
        FetchedItems GetItemsForSelectionPath(string selectionPath);
        void AddItemToObject(FetchedItem item, IFunctionSupportedValue value);
    }
}