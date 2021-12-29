using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Core.Models.Path;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class FunctionConverter : JsonConverter
    {
        private readonly IFunctionsProvider provider;

        public FunctionConverter(IFunctionsProvider functionsProvider)
        {
            provider = functionsProvider ?? throw new ArgumentNullException(nameof(functionsProvider));
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var functionValue = (FunctionSupportedValue) value;
            JToken tokenToWrite;
            if (functionValue.Function is FixedValue f)
                tokenToWrite = GetFixedValueToken(f, functionValue);
            else
                tokenToWrite =
                    JToken.Parse($"\"={((IFunctionSupportedValue) value).GetStringRepresentation()}\"");
            tokenToWrite.WriteTo(writer);
        }

        private JToken GetFixedValueToken(FixedValue fixedValue, IFunctionSupportedValue value)
        {
            if (fixedValue.Value.Type != JTokenType.String) return fixedValue.Value;
            return value.GetStringRepresentation();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = JToken.Load(reader);
            if (value.Type == JTokenType.String) return ParseString(value.ToString());

            return new FunctionSupportedValue(new FixedValue(value));
        }

        private IFunctionSupportedValue ParseString(string text)
        {
            if (string.IsNullOrEmpty(text)) return new FunctionSupportedValue(new FixedValue(JValue.CreateNull()));
            if (!text.StartsWith(CoreConstants.FunctionStartCharacters))
                return new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{text}\"")));
            var (function, arguments) = GetFunctionAndArguments(text);

            return new FunctionSupportedValue(function.SetArguments(arguments));
        }

        private (IFunction function, Arguments arguments) GetFunctionAndArguments(string text)
        {
            var mainSplit = SplitText.GetChoppedElements(text,
                new[] {CoreConstants.FunctionArgumentsStartCharacters, CoreConstants.FunctionArgumentsEndCharacters},
                CoreConstants.ArgumentLevelPairs);
            var functionName = mainSplit[0].Text.TrimStart(CoreConstants.FunctionArgumentsStartCharacters)
                .Trim(CoreConstants.FunctionStartCharacters.ToCharArray());

            var function = provider[functionName];
            if (mainSplit.Count > 1 && function != null)
            {
                var argumentSection = CleanArguments(mainSplit[1].Text);
                return DiscoverFunctionsUsedInArguments(function, argumentSection);
            }

            return (new FixedValue(new JValue(text)), new Arguments());
        }

        private string CleanArguments(string text)
        {
            var index = text.IndexOf(CoreConstants.FunctionArgumentsEndCharacters);
            if (index >= 0)
                return text.Substring(0, index);
            return text;
        }

        private (IFunction function, Arguments arguments) DiscoverFunctionsUsedInArguments(IFunction function,
            string argumentsText)
        {
            var functionsArguments = new Arguments();
            SplitText.GetChoppedElements(argumentsText, CoreConstants.ArgumentsDelimiter,
                CoreConstants.ArgumentLevelPairs).ForEach(i =>
            {
                var argumentAnalysis = GetFunctionAndArguments(i.Text);
                functionsArguments.Add(
                    new FunctionSupportedValue(argumentAnalysis.function.SetArguments(argumentAnalysis.arguments)));
            });
            return (function, functionsArguments);
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IFunctionSupportedValue).IsAssignableFrom(objectType) ||
                   objectType is IFunctionSupportedValue ||
                   objectType == typeof(IFunctionSupportedValue);
        }
    }
}