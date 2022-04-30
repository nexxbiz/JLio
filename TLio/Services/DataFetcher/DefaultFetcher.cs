using System;
using System.Collections.Generic;
using System.Text.Json;
using TLio.Contracts.DataFetcher;

namespace TLio.Services.DataFetcher
{
    public class DefaultFetcher : IDataFetcher
    {
        public FetchedItems GetItemsForPath(string path, IReadOnlyDictionary<string, object> contextInput)
        {
            try
            {
                var serializedInput = JsonSerializer.Serialize(contextInput);
            }
            catch (Exception ex)
            {
                throw new Exception("Can not serialize input", ex);
            }

            return new FetchedItems()
            {
                new FetchedItem
                {
                    Item = { },
                    Path = "$",
                    ItemType = TargetTypes.Object
                }
            };
        }
    }
}