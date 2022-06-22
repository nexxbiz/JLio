using Json.Path;
using System.Text.Json;
using System.Text.Json.Nodes;
using TLio.Contracts.DataFetcher;

namespace TLio.Services.DataFetcher
{
    public class DefaultFetcher : IDataFetcher
    {
        public object? GetExecutionInput(IReadOnlyDictionary<string, object> input)
        {

            var result = new JsonObject();
            input.Keys.ToList().ForEach(key => result.Add(key,((JsonNode)input[key])));
            return result;
        }

        public FetchedItems GetItemsForParentPath(string path, object? input)
        {
          var parentPath = path.Substring(0, path.LastIndexOf('.'));
            return GetItemsForPath(parentPath, input);
        }

        public FetchedItems GetItemsForPath(string path, object? contextInput)
        {
            var result = new FetchedItems();
            var items = JsonPath.Parse(path).Evaluate(JsonDocument.Parse(contextInput.ToString()!).RootElement);

            items.Matches?.ToList().ForEach(item => result.Add(new FetchedItem { Item= item, Path = item.Location.ToString(), ItemType = TargetTypes.Array }));

            return result;
          
        }
    }
}