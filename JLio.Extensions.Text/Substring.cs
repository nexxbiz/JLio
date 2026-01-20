using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class Substring : FunctionBase
{
    public Substring()
    {
    }

    public Substring(params string[] arguments)
    {
        foreach (var a in arguments)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));
        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count < 2 || values.Count > 3)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 2 or 3 arguments (text, startIndex, [length])");
            return JLioFunctionResult.Failed(currentToken);
        }

        var text = TextHelpers.GetStringValue(values[0]);
        
        if (!TextHelpers.TryGetIntegerValue(values[1], out var startIndex))
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} startIndex must be a valid integer");
            return JLioFunctionResult.Failed(currentToken);
        }

        // Handle negative start index (from end)
        if (startIndex < 0)
            startIndex = text.Length + startIndex;

        if (startIndex < 0 || startIndex >= text.Length)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} startIndex {startIndex} is out of bounds for string of length {text.Length}");
            return JLioFunctionResult.Failed(currentToken);
        }

        string result;
        if (values.Count == 3)
        {
            if (!TextHelpers.TryGetIntegerValue(values[2], out var length))
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} length must be a valid integer");
                return JLioFunctionResult.Failed(currentToken);
            }

            if (length < 0)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} length cannot be negative");
                return JLioFunctionResult.Failed(currentToken);
            }

            var availableLength = text.Length - startIndex;
            var actualLength = Math.Min(length, availableLength);
            result = text.Substring(startIndex, actualLength);
        }
        else
        {
            result = text.Substring(startIndex);
        }

        return new JLioFunctionResult(true, new JValue(result));
    }
}