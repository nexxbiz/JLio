using System;
using JLio.Core.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class JLioCommandConverter : JsonConverter
    {
        private readonly IJLioCommandsProvider provider;

        public JLioCommandConverter(IJLioCommandsProvider provider)
        {
            this.provider = provider;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken.Parse(JsonConvert.SerializeObject(value)).WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var json = JObject.Load(reader);
            var discriminatorField = json.Property(JLioConstants.CommandDiscriminator)?.Value.ToString();
            if (discriminatorField == null) {  return null; }
            var foundCommand = provider[discriminatorField];
            if (foundCommand == null) return new NotFoundCommand(json.ToString());
            serializer.Populate(json.CreateReader(), foundCommand);
            return foundCommand;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(IJLioCommand));
        }
    }
}