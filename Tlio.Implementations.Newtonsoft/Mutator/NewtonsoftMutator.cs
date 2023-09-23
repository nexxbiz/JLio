using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TLio.Contracts.Mutator;
using TLio.Services.DataFetcher;

namespace Tlio.Implementations.Newtonsoft.Mutator
{
    public class NewtonsoftMutator : IMutator
    {
        public void AddValueToArray(FetchedItem item, object value, string? propertyName = "")
        {
            throw new NotImplementedException();
        }

        public void AddValueToObject(FetchedItem item, object value, string propertyName)
        {
            var parsedValue = JToken.Parse(JsonConvert.SerializeObject(value));

            var obj = item.Item as JObject;
            if (obj != null)
            {
                obj.Add(propertyName, parsedValue);
            }
        }
    }
}

