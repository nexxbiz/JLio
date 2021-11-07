using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public class Promote : FunctionBase
    {
        public Promote()
        {
        }

        public Promote(string newPropertyName)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{newPropertyName}\""))));
        }

        public Promote(string path, string newPropertyName)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{path}\""))));
            Arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{newPropertyName}\""))));
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
                return DoPromote(values[0].ToString().Trim(CoreConstants.StringIndicator), currentToken);

            return DoPromote(values[1].ToString().Trim(CoreConstants.StringIndicator), values[0]);
        }

        private static JLioFunctionResult DoPromote(string propertyName, JToken value)
        {
            var newToken = new JObject {{propertyName, value}};
            return JLioFunctionResult.SuccessFul(newToken);
        }
    }
}