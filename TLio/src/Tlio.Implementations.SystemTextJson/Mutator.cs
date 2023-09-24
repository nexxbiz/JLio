
using System.Text.Json.Nodes;
using TLio.Contracts.Mutator;
using TLio.Services.DataFetcher;

namespace TLio.Implementations.SystemTextJson
{
    public class Mutator : IMutator<JsonNode>
    {
        public void AddValueToArray(FetchedItem<JsonNode> item, JsonNode value, string? propertyName = "")
        {
            throw new NotImplementedException();
        }

        public void AddValueToObject(FetchedItem<JsonNode> item, JsonNode value, string propertyName)
        {
            //var parsedValue = JToken.Parse(JsonConvert.SerializeObject(value));

            //var obj = item.Item as JObject;
            //if (obj != null)
            //{
            //    obj.Add(propertyName, parsedValue);
            //}
        }
    }
}

