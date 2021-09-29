using JLio.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class FunctionSupportedValue : IFunctionSupportedValue
    {
        public FunctionSupportedValue(IFunction function)
        {
            Function = function;
        }

        public IFunction Function { get; }

        public SelectedTokens GetValue(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            var result = Function.Execute(currentToken, dataContext, options);
            if (result.Success == false)
            {
            }

            return result.Data;
        }

        public string GetStringRepresentation()
        {
            return Function.ToScript();
        }
    }
}