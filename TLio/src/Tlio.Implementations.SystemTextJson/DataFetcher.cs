using Json.Path;
using System.Text.Json.Nodes;
using TLio.Contracts.DataFetcher;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.SystemTextJson
{
    public class DataFetcher : IDataFetcher<JsonNode>
    {
        public JsonNode? GetExecutionInput(IReadOnlyDictionary<string, JsonNode> input)
        {
               return JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(input));
        }

        public FetchedItems<JsonNode> GetItemsForParentPath(string path, JsonNode? input)
        {
            if (IsRootPath(path))
            {
                return new FetchedItems<JsonNode>()
                {
                    new FetchedItem<JsonNode>
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

        public FetchedItems<JsonNode> GetItemsForPath(string path, JsonNode? contextInput)
        {
            var result = new FetchedItems<JsonNode>();
           
            var jsonPath = JsonPath.Parse(path);

           
            foreach (var item in jsonPath.Evaluate(contextInput).Matches)
            {
                result.Add(new FetchedItem<JsonNode> { Item = item?.Value, Path = item?.Location?.ToString()??string.Empty, ItemType = ConversionHelper.GetTargetType(item) });
            }


            return result;
        }

        public Dictionary<string, JsonNode> GetExecutionResult(JsonNode? result)
        {
            var executionResult = new Dictionary<string, JsonNode>();
            var data = result as JsonNode;

            //foreach (var property in data?.Properties())
            //{
            //    executionResult.Add(property.Name, property.Value);
            //}

            return executionResult;
        }
    }
}
