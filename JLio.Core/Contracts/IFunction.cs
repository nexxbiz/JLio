using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface IFunction
    {
        string FunctionName { get; }

        IFunction SetArguments(Arguments arguments);

        JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context);

        string ToScript();
    }

    public class JLioFunctionResult
    {
        public JLioFunctionResult(bool success, SelectedTokens data)
        {
            Success = success;
            Data = data;
        }

        public JLioFunctionResult(bool success, JToken data)
        {
            Success = success;
            Data = new SelectedTokens(data);
        }

        public SelectedTokens Data { get; }

        public bool Success { get; }

        public static JLioFunctionResult Failed(SelectedTokens data)
        {
            return new JLioFunctionResult(false, data);
        }

        public static JLioFunctionResult SuccessFul(SelectedTokens data)
        {
            return new JLioFunctionResult(true, data);
        }

        public static JLioFunctionResult Failed(JToken data)
        {
            return new JLioFunctionResult(false, new SelectedTokens(data));
        }

        public static JLioFunctionResult SuccessFul(JToken data)
        {
            return new JLioFunctionResult(true, new SelectedTokens(data));
        }
    }
}