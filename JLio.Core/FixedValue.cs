using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class FixedValue : IFunction
    {
        private readonly JToken value;

        public FixedValue(JToken value)
        {
            this.value = value;
        }

        public string FunctionName => "FixedValue";

        public IFunction SetArguments(Arguments newArguments)
        {
            return this;
        }

        public JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            if (value.Type == JTokenType.String) return HandleString(currentToken, dataContext, options);
            return new JLioExecutionResult(true, value);
        }

        public string ToScript()
        {
            return value.ToString();
        }

        private JLioExecutionResult HandleString(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            var stringValue = value.ToString();

            if (stringValue.StartsWith(JLioConstants.CurrentItemPathIndicator, StringComparison.InvariantCulture))
                return new JLioExecutionResult(true,
                    options.ItemsFetcher.SelectToken(
                        stringValue.Replace(JLioConstants.CurrentItemPathIndicator,
                            JLioConstants.RootPathIndicator), currentToken));
            if (stringValue.StartsWith(JLioConstants.RootPathIndicator, StringComparison.InvariantCulture))
                return new JLioExecutionResult(true, options.ItemsFetcher.SelectToken(stringValue, dataContext));
            return new JLioExecutionResult(true, value);
        }
    }
}