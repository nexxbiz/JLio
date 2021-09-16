using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class FixedValue : IFunction
    {
        public FixedValue(JToken value)
        {
            Value = value;
        }

        public JToken Value { get; }

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

            if (stringValue.StartsWith(options.ItemsFetcher.CurrentItemPathIndicator,
                StringComparison.InvariantCulture))
                return new JLioExecutionResult(true,
                    options.ItemsFetcher.SelectToken(
                        stringValue.Replace(options.ItemsFetcher.CurrentItemPathIndicator,
                            options.ItemsFetcher.RootPathIndicator), currentToken));
            if (stringValue.StartsWith(options.ItemsFetcher.RootPathIndicator, StringComparison.InvariantCulture))
                return new JLioExecutionResult(true, options.ItemsFetcher.SelectToken(stringValue, dataContext));
            return new JLioExecutionResult(true, Value);
        }
    }
}