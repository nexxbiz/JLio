using System;
using System.Diagnostics;
using JLio.Core.Contracts;
using JLio.Core.Models;
using JLio.Core.Models.Path;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class JLioFunctionConverter : JsonConverter
    {
        private readonly IJLioFunctionsProvider provider;

        public JLioFunctionConverter(IJLioFunctionsProvider functionsProvider)
        {
            provider = functionsProvider ?? throw new ArgumentNullException(nameof(functionsProvider));
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var functionValue = (JLioFunctionSupportedValue) value;
            JToken tokenToWrite;
            if (functionValue.Function is FixedValue f)
            {
                tokenToWrite = GetFixedValueToken(f, functionValue);
            }
            else
            {
                tokenToWrite =
                    JToken.Parse($"\"={((JLioFunctionSupportedValue)value).GetStringRepresentation()}\"");
            }
            tokenToWrite.WriteTo(writer);
        }

        private JToken GetFixedValueToken(FixedValue fixedValue, JLioFunctionSupportedValue value)
        {
            if(fixedValue.Value.Type != JTokenType.String)
            {
                return fixedValue.Value;
            }
               return  value.GetStringRepresentation();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = JToken.Load(reader);
            if (value.Type == JTokenType.String) return ParseString(value.ToString());

            return new JLioFunctionSupportedValue(new FixedValue(value));
        }

        private IFunctionSupportedValue ParseString(string text)
        {
            if (string.IsNullOrEmpty(text)) return new JLioFunctionSupportedValue(new FixedValue(JValue.CreateNull()));
            if (!text.StartsWith(JLioConstants.FunctionStartCharacters))
                return new JLioFunctionSupportedValue(new FixedValue(JToken.Parse($"\"{text}\"")));
            var (function, arguments) = GetFunctionAndArguments(text);

            return new JLioFunctionSupportedValue(function.SetArguments(arguments));
        }

        private (IFunction function, Arguments arguments) GetFunctionAndArguments(string text)
        {
            var mainSplit = SplitText.GetChoppedElements(text,
                new[] {JLioConstants.FunctionArgumentsStartCharacters, JLioConstants.FunctionArgumentsEndCharacters},
                JLioConstants.ArgumentLevelPairs);
            var functionName = mainSplit[0].Text.TrimStart(JLioConstants.FunctionArgumentsStartCharacters).Trim(JLioConstants.FunctionStartCharacters.ToCharArray());

            var function = provider[functionName];
            if (mainSplit.Count > 1 && function != null)
                return DiscoverFunctionsUsedInArguments(function, mainSplit[1].Text);
            return (new FixedValue(new JValue(text)), new Arguments());
        }

        private (IFunction function, Arguments arguments) DiscoverFunctionsUsedInArguments(IFunction function,
            string argumentsText)
        {
            var functionsArguments = new Arguments();
            SplitText.GetChoppedElements(argumentsText, JLioConstants.ArgumentsDelimiter,
                JLioConstants.ArgumentLevelPairs).ForEach(i =>
            {
                var argumentAnalysis = GetFunctionAndArguments(i.Text);
                functionsArguments.Add(
                    new JLioFunctionSupportedValue(argumentAnalysis.function.SetArguments(argumentAnalysis.arguments)));
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