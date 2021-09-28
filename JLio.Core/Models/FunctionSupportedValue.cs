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

        public JToken GetValue(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            var result = Function.Execute(currentToken, dataContext, options);
            if (result.Data == null) return JValue.CreateNull();
            if (result.Data.Type == JTokenType.String)
                return result.Data.ToString().Trim(CoreConstants.StringIndicator);
            return result.Data;
        }

        public string GetStringRepresentation()
        {
            return Function.ToScript();
        }
    }
}