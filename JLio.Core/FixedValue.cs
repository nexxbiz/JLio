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

        public JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            if (Value.Type == JTokenType.String) return HandleString(currentToken, dataContext, context);
            return new JLioExecutionResult(true, Value);
        }

        public string ToScript()
        {
            return Value.ToString();
        }

        private JLioExecutionResult HandleString(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            var stringValue = Value.ToString();

            if (stringValue.StartsWith(context.ItemsFetcher.CurrentItemPathIndicator,
                StringComparison.InvariantCulture))
                return new JLioExecutionResult(true,
                    context.ItemsFetcher.SelectToken(
                        stringValue.Replace(context.ItemsFetcher.CurrentItemPathIndicator,
                            context.ItemsFetcher.RootPathIndicator), currentToken));
            if (stringValue.StartsWith(context.ItemsFetcher.RootPathIndicator, StringComparison.InvariantCulture))
                return new JLioExecutionResult(true, context.ItemsFetcher.SelectToken(stringValue, dataContext));
            return new JLioExecutionResult(true, Value);
        }
    }
}