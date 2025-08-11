using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class PadRight : FunctionBase
{
    public PadRight()
    {
    }

    public PadRight(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count < 2 || values.Count > 3)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 2 or 3 arguments (text, totalWidth, [padChar])");
            return JLioFunctionResult.Failed(currentToken);
        }

        var text = TextHelpers.GetStringValue(values[0]);
        
        if (!TextHelpers.TryGetIntegerValue(values[1], out var totalWidth) || totalWidth < 0)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} totalWidth must be a non-negative integer");
            return JLioFunctionResult.Failed(currentToken);
        }

        char padChar = ' ';
        if (values.Count == 3)
        {
            var padCharString = TextHelpers.GetStringValue(values[2]);
            if (!string.IsNullOrEmpty(padCharString))
            {
                padChar = padCharString[0];
            }
        }

        var result = text.PadRight(totalWidth, padChar);
        return new JLioFunctionResult(true, new JValue(result));
    }
}