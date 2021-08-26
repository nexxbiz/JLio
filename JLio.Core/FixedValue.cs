using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class FixedValue : IFunction
    {
        private readonly JToken value;
        private Arguments arguments;

        public FixedValue(JToken value)
        {
            this.value = value;
        }

        public string FunctionName => "FixedValue";

        public IFunction SetArguments(Arguments newArguments)
        {
            arguments = newArguments;
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

            if (stringValue.StartsWith(JLioConstants.CurrentItemPathIndicator, System.StringComparison.InvariantCulture))
            {
                return new JLioExecutionResult(true,
                       options.ItemsFetcher.SelectToken(
                           stringValue.Replace(JLioConstants.CurrentItemPathIndicator,
                               JLioConstants.RootPathIndicator), currentToken));
            }
            else if (stringValue.StartsWith(JLioConstants.RootPathIndicator, System.StringComparison.InvariantCulture))
            {
                return new JLioExecutionResult(true, options.ItemsFetcher.SelectToken(stringValue, dataContext));
            }
            return new JLioExecutionResult(true, value);
        }
    }
}