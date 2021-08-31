using JLio.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class JLioFunctionSupportedValue : IFunctionSupportedValue
    {
        public JLioFunctionSupportedValue(IFunction function)
        {
            Function = function;
        }

        public IFunction Function { get; }

        public JToken GetValue(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            var result = Function.Execute(currentToken, dataContext, options);
            if (result.Data.Type == JTokenType.String)
                return result.Data.ToString().Trim(JLioConstants.StringIndicator);
            return result.Data;
        }

        public string GetStringRepresentation()
        {
            return Function.ToScript();
        }
    }
}