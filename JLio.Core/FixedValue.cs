using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class FixedValue : IFunction
    {
        public JToken Value { get; }

        public FixedValue(JToken value)
        {
            Value = value;
        }

        public string FunctionName => "FixedValue";

        public IFunction SetArguments(Arguments newArguments)
        {
            return this;
        }

        public JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            if (Value.Type == JTokenType.String) return HandleString(currentToken, dataContext, options);
            return new JLioExecutionResult(true, Value);
        }

        public string ToScript()
        {
            return Value.ToString();
        }

        private JLioExecutionResult HandleString(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            var stringValue = Value.ToString();

            if (stringValue.StartsWith(JLioConstants.CurrentItemPathIndicator, StringComparison.InvariantCulture))
                return new JLioExecutionResult(true,
                    options.ItemsFetcher.SelectToken(
                        stringValue.Replace(JLioConstants.CurrentItemPathIndicator,
                            JLioConstants.RootPathIndicator), currentToken));
            if (stringValue.StartsWith(JLioConstants.RootPathIndicator, StringComparison.InvariantCulture))
                return new JLioExecutionResult(true, options.ItemsFetcher.SelectToken(stringValue, dataContext));
            return new JLioExecutionResult(true, Value);
        }
    }
}