using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IFunctionSupportedValue
    {
        IFunction Function { get; }
        SelectedTokens GetValue(JToken currentToken, JToken dataContext, IExecutionContext context);
        string GetStringRepresentation();
    }
}