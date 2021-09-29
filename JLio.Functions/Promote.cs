using System;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
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
            arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{newPropertyName}\""))));
        }

        public Promote(string path, string newPropertyName)
        {
            arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{path}\""))));
            arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{newPropertyName}\""))));
        }

        public override JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            if (arguments.Count == 0 || arguments.Count > 2)
            {
                options.Logger.Log(LogLevel.Error, CoreConstants.FunctionExecution,
                    $"failed: {FunctionName} requires 2 arguments (path, newPropertyName)");
                return JLioExecutionResult.Failed(currentToken);
            }

            var values = GetArguments(arguments, currentToken, dataContext, options);
            try
            {
                if (values.Count == 1)
                    return DoPromote(values[0].ToString().Trim(CoreConstants.StringIndicator), currentToken);

                return DoPromote(values[1].ToString().Trim(CoreConstants.StringIndicator), values[0]);
            }
            catch (Exception ex)
            {
                options.Logger.Log(LogLevel.Error, CoreConstants.FunctionExecution, $"failed: {ex.Message}");
                return JLioExecutionResult.Failed(currentToken);
            }
        }

        private static JLioExecutionResult DoPromote(string propertyName, JToken value)
        {
            var newToken = new JObject {{propertyName, value}};
            return JLioExecutionResult.SuccessFull(newToken);
        }
    }
}