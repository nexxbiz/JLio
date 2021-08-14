using JLio.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class JLioFunctionSupportedValue : IJLioFunctionSupportedValue
    {
        public JLioFunctionSupportedValue(IJLioFunction function)
        {
            Function = function;
        }

        public IJLioFunction Function { get; }

        public JToken GetValue(JToken currentToken, JToken dataContext, IJLioExecutionOptions options)
        {
            var result = Function.Execute(currentToken, dataContext, options);
            return result.Data;
        }

        public string GetStringRepresentation()
        {
            return Function.ToScript();
        }
    }
}