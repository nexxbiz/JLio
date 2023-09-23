using Json.Path;
using System.Text.Json.Nodes;
using TLio.Contracts.DataFetcher;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.SystemTextJson
{
    public class DataFetcher : IDataFetcher
    {
        public object? GetExecutionInput(IReadOnlyDictionary<string, object> input)
        {
               return JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(input));
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
                        Item = JsonNode.Parse("{}")
                    }
                };
            }
            else
            {
                var parentPath = path.Substring(0, path.LastIndexOf('.'));
                return GetItemsForPath(parentPath, input);
            }
        }

        private bool IsRootPath(string path)
        {
            return path == "$";
        }

        public FetchedItems GetItemsForPath(string path, object? contextInput)
        {
            var result = new FetchedItems();
            var items = contextInput as JsonNode;
            var jsonPath = JsonPath.Parse(path);

           
            foreach (var item in jsonPath.Evaluate(items).Matches)
            {
                result.Add(new FetchedItem { Item = item, Path = item.Location.ToString(), ItemType = ConversionHelper.GetTargetType(item) });
            }


            return result;
        }

        public Dictionary<string, object> GetExecutionResult(object? result)
        {
            var executionResult = new Dictionary<string, object>();
            var data = result as JsonNode;

            //foreach (var property in data?.Properties())
            //{
            //    executionResult.Add(property.Name, property.Value);
            //}

            return executionResult;
        }
    }
}
