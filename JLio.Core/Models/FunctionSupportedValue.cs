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

        public SelectedTokens GetValue(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            var result = Function.Execute(currentToken, dataContext, context);
            if (result.Success == false)
                context.LogError(CoreConstants.FunctionExecution, "Execute of function failed");

            return result.Data;
        }

        public string GetStringRepresentation()
        {
            return Function.ToScript();
        }
    }
}