using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLio.Contracts.DataFetcher;
using TLio.Implementations.Newtonsoft.Helpers;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.Newtonsoft.DataFetcher
{
    public class NewtonsoftDataFetcher : IDataFetcher
    {
        public object? GetExecutionInput(IReadOnlyDictionary<string, object> input)
        {
           return JToken.Parse(JsonConvert.SerializeObject(input));
        }

        public FetchedItems GetItemsForParentPath(string path, object? input)
        {
            if (IsRootPath(path))
            {
                return new FetchedItems()
                {
                    new FetchedItem
                    {
                        ItemType = TargetTypes.Object,
                        Path = path,
                        Item = JToken.Parse("{}")
                    }
                };
            }
            else
            {
                var parentPath = path.Substring(0, path.LastIndexOf('.'));
                return GetItemsForPath(parentPath, input);
            }
        }

        public FetchedItems GetItemsForPath(string path, object? contextInput)
        {
            var result = new FetchedItems();
            var items = contextInput as JToken;

            var matchedItems = items.SelectTokens(path);

            foreach (var item in matchedItems)
            {
                result.Add(new FetchedItem { Item = item, Path = item.Path, ItemType = ConversionHelper.GetTargetType(item) });
            }


            return result;
        }

        //TODO: Move to commons so it can be reused
        private static bool IsRootPath(string jsonPath)
        {
            return jsonPath == "$";
        }

        public Dictionary<string, object> GetExecutionResult(object? result)
        {
            var executionResult = new Dictionary<string, object>();
            var data = result as JObject;

            foreach (var property in data?.Properties())
            {
                executionResult.Add(property.Name, property.Value);
            }

            return executionResult;
        }
    }
}
