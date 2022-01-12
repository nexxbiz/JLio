using System;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public class Format : FunctionBase
    {
        public Format()
        {
        }

        public Format(string formatString)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(formatString)));
        }

        public Format(string path, string formatString)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{path}\""))));
            Arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{formatString}\""))));
        }

        public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            if (Arguments.Count == 0 || Arguments.Count > 2)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"failed: {FunctionName} requires 2 arguments (path, newPropertyName)");
                return JLioFunctionResult.Failed(currentToken);
            }

            var values = GetArguments(Arguments, currentToken, dataContext, context);
            if (values.Count == 1)
                return DoFormat(values[0].ToString().Trim(CoreConstants.StringIndicator), currentToken);

            return DoFormat(values[1].ToString().Trim(CoreConstants.StringIndicator), values[0]);
        }

        private static JLioFunctionResult DoFormat(string formatString, JToken value)
        {
            if (value.Type == JTokenType.Date)
            {
                var test = value.Value<DateTime>();
                return JLioFunctionResult.SuccessFul(new JValue(test.ToString(formatString)));
            }

            return JLioFunctionResult.SuccessFul(value);
        }
    }
}