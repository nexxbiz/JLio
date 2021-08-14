using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IJLioFunction
    {
        string FunctionName { get; }

        IJLioFunction SetArguments(Arguments arguments);

        JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IJLioExecutionOptions options);

        string ToScript();
    }
}