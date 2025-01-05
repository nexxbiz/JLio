using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.UnitTests.CommandsTestV2
{
    public static partial class TestCaseLoader
    {
        private class JTokenConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(JToken);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                => JToken.Load(reader);

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                => (value as JToken)?.WriteTo(writer);
        }
    }
}
