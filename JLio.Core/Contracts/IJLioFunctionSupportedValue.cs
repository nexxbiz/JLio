using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IJLioFunctionSupportedValue
    {
        IJLioFunction Function { get; }
        JToken GetValue(JToken currentToken, JToken dataContext, IJLioExecutionOptions options);
        string GetStringRepresentation();
    }
}