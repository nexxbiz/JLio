using Lio.Core.Models;

namespace Lio.Core.Contracts
{
    public interface ISpecificMutator
    {
        void AddValueToArray(FetchedItem item, IFunctionSupportedValue value);

        void AddValueToObject(FetchedItem item, IFunctionSupportedValue value);
    }
}