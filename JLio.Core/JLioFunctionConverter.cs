using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Core.Models.Path;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class JLioFunctionConverter : JsonConverter
    {
        private readonly string functionStartCharacters = JLioConstants.FunctionStartCharacters;
        private readonly IJLioFunctionsProvider provider;

        public JLioFunctionConverter(IJLioFunctionsProvider functionsProvider)
        {
            provider = functionsProvider ?? throw new ArgumentNullException(nameof(functionsProvider));
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var functionValue = (JLioFunctionSupportedValue) value;
            JToken stringRepresentation;
            if (functionValue.Function.GetType() == typeof(FixedValue))
                stringRepresentation = new JValue(functionValue.GetStringRepresentation());
            else
                stringRepresentation =
                    JToken.Parse($"\"={((JLioFunctionSupportedValue) value).GetStringRepresentation()}\"");

            stringRepresentation.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = JToken.Load(reader);
            if (value.Type == JTokenType.String) return ParseString(value.ToString());

            return value;
        }

        private IJLioFunctionSupportedValue ParseString(string text)
        {
            if (string.IsNullOrEmpty(text)) return new JLioFunctionSupportedValue(new FixedValue(JValue.CreateNull()));
            if (!text.StartsWith(functionStartCharacters))
                return new JLioFunctionSupportedValue(new FixedValue(JToken.Parse($"\"{text}\"")));
            text = text.Substring(functionStartCharacters.Length);
            var analysis = GetFunctionAndArguments(text);
            return new JLioFunctionSupportedValue(analysis.function.SetArguments(analysis.arguments));
        }

        private (IJLioFunction function, Arguments arguments) GetFunctionAndArguments(string text)
        {
            var mainSplit = SplitText.GetChoppedElements(text,
                new[] {JLioConstants.FunctionArgumentsStartCharacters, JLioConstants.FunctionArgumentsEndCharacters},
                JLioConstants.ArgumentLevelPairs);
            var functionName = mainSplit[0].Text;
            var arguments = new Arguments();
            var function = provider[functionName];
            return GetFunctionsFromArguments(text, function, arguments, mainSplit);
        }

        private (IJLioFunction function, Arguments arguments) GetFunctionsFromArguments(string text,
            IJLioFunction function,
            Arguments arguments, ChoppedElements mainSplit)
        {
            if (function == null) return (new FixedValue(new JValue(text)), arguments);

            SplitText.GetChoppedElements(mainSplit[1].Text, JLioConstants.ArgumentsDelimiter,
                JLioConstants.ArgumentLevelPairs).ForEach(i =>
            {
                var analysis = GetFunctionAndArguments(i.Text);
                arguments.Add(
                    new JLioFunctionSupportedValue(analysis.function.SetArguments(analysis.arguments)));
            });
            return (function, arguments);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(IJLioFunctionSupportedValue)) ||
                   objectType == typeof(IJLioFunctionSupportedValue);
        }
    }
}