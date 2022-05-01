using TLio.Contracts.Mutator;
using TLio.Implementations;
using TLio.Services.DataFetcher;

namespace TLio.Services.Mutator
{
    public class DefaultMutator : IMutator
    {
        public void AddValueToObject(FetchedItem targetItem, IValue value)
        {
            if(targetItem.ItemType != TargetTypes.Object)
            {

            }
           
        }

        public void AddValueToArray(FetchedItem item, IValue value)
        {
            throw new System.NotImplementedException();
        }
    }
}