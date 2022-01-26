using System.Collections.Generic;

namespace Lio.Core.Models
{
    public class FetchedItems : List<FetchedItem>
    {
    }

    public class FetchedItem
    {
        public string Path { get; set; }
        public TargetTypes TargetType { get; set; }
    }
}