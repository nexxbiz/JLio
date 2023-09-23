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

            //var result = new JsonObject();
            // input.Keys.ToList().ForEach(key => result.Add(key, input[key]));

            JsonElement jsonElement = ConvertToJsonElement(input);
            return jsonElement;
        }

        public static JsonElement ConvertToJsonElement(IReadOnlyDictionary<string, object> dictionary)
        {
            string json = JsonSerializer.Serialize(dictionary);

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.Clone(); // Use Clone() to get a copy of the JsonElement that will persist outside of the using block.
        }

        public FetchedItems GetItemsForParentPath(string path, object? input)
        {
            //TODO: fix this as the path might be simply $
            var parentPath = path.Substring(0, path.LastIndexOf('.'));
            return GetItemsForPath(parentPath, input);
        }

        public static JsonNode ConvertToJsonNode(object obj)
        {
            string json = JsonSerializer.Serialize(obj);
            return JsonNode.Parse(json);
        }

        public FetchedItems GetItemsForPath(string path, object? contextInput)
        {
            var result = new FetchedItems();
            var items = JsonPath.Parse(path).Evaluate(ConvertToJsonNode(contextInput));

            items.Matches?.ToList().ForEach(item => result.Add(new FetchedItem { Item = item, Path = item.Location.ToString(), ItemType = GetTargetType(item) }));

            return result;

        }

        private TargetTypes GetTargetType(Node item)
        {

            switch (item.Value)
            {
                case JsonObject obj:
                    return TargetTypes.Object;
                default: return TargetTypes.Undefined;
            }

            //switch (test)
            //{
            //    case JsonValueKind.Undefined:
            //        return TargetTypes.Undefined;
            //    case JsonValueKind.Object:
            //        return TargetTypes.Object;
            //    case JsonValueKind.Array:
            //        return TargetTypes.Array;
            //    case JsonValueKind.String:
            //        return TargetTypes.String;
            //    case JsonValueKind.Number:
            //        if (item.GetType() == typeof(int))
            //            return TargetTypes.Integer;
            //        else
            //            return TargetTypes.Float;
            //    case JsonValueKind.True:
            //        return TargetTypes.Boolean;
            //    case JsonValueKind.False:
            //        return TargetTypes.Boolean;
            //    case JsonValueKind.Null:
            //        return TargetTypes.Null;
            //    default:
            //        return TargetTypes.None;
            //}
        }

        public Dictionary<string, object?> GetExecutionResult(object? result)
        {
            throw new NotImplementedException();
        }
    }
}