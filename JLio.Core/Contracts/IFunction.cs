using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IFunction
    {
        string FunctionName { get; }

        IFunction SetArguments(Arguments arguments);

        JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options);

        string ToScript();
    }
}