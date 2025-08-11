using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class IndexOf : FunctionBase
{
    public IndexOf()
    {
    }

    public IndexOf(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count < 2 || values.Count > 4)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 2 to 4 arguments (text, searchValue, [startIndex], [ignoreCase])");
            return JLioFunctionResult.Failed(currentToken);
        }

        var text = TextHelpers.GetStringValue(values[0]);
        var searchValue = TextHelpers.GetStringValue(values[1]);
        
        var startIndex = 0;
        if (values.Count >= 3)
        {
            if (!TextHelpers.TryGetIntegerValue(values[2], out startIndex) || startIndex < 0)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} startIndex must be a non-negative integer");
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        var ignoreCase = false;
        if (values.Count == 4)
        {
            if (!TextHelpers.TryGetBooleanValue(values[3], out ignoreCase))
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} ignoreCase must be a valid boolean");
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        if (startIndex >= text.Length)
        {
            return new JLioFunctionResult(true, new JValue(-1));
        }

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var result = text.IndexOf(searchValue, startIndex, comparison);

        return new JLioFunctionResult(true, new JValue(result));
    }
}