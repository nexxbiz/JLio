using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IFunctionSupportedValue
    {
        IFunction Function { get; }
        JToken GetValue(JToken currentToken, JToken dataContext, IExecutionContext context);
        string GetStringRepresentation();
    }
}