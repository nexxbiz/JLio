using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class StartsWith : FunctionBase
{
    public StartsWith()
    {
    }

    public StartsWith(params string[] arguments)
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
                $"{FunctionName} requires 2 or 3 arguments (text, prefix, [ignoreCase])");
            return JLioFunctionResult.Failed(currentToken);
        }

        var text = TextHelpers.GetStringValue(values[0]);
        var prefix = TextHelpers.GetStringValue(values[1]);
        
        var ignoreCase = false;
        if (values.Count == 3)
        {
            if (!TextHelpers.TryGetBooleanValue(values[2], out ignoreCase))
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} ignoreCase must be a valid boolean");
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var result = text.StartsWith(prefix, comparison);

        return new JLioFunctionResult(true, new JValue(result));
    }
}