using System.Collections.Generic;

namespace Lio.Core.Models
{
    public class FetchedItems : List<FetchedItem>
    {
        public eFetchedResult FetchResult { get; private set; } = eFetchedResult.Failed;

        public static FetchedItems Failed()
        {
            return new FetchedItems
            {
                FetchResult = eFetchedResult.Failed
            };
        }

        public static FetchedItems Success(IEnumerable<FetchedItem> items)
        {
            var result = new FetchedItems
            {
                FetchResult = eFetchedResult.Success
            };
            result.AddRange(items);
            return result;
        }

        public static FetchedItems Warning(IEnumerable<FetchedItem> items)
        {
            var result = new FetchedItems
            {
                FetchResult = eFetchedResult.Warnings
            };
            result.AddRange(items);
            return result;
        }
    }

    public enum eFetchedResult
    {
        Failed,
        Warnings,
        Success
    }

    public class FetchedItem
    {
        public object Item { get; set; }
        public TargetTypes ItemType { get; set; }
        public string Path { get; set; }
    }
}