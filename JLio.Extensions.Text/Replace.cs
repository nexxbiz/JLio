using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class Replace : FunctionBase
{
    public Replace()
    {
    }

    public Replace(params string[] arguments)
    {
        foreach (var a in arguments)

        {

            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));

        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count < 3 || values.Count > 4)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 3 or 4 arguments (text, oldValue, newValue, [ignoreCase])");
            return JLioFunctionResult.Failed(currentToken);
        }

        var text = TextHelpers.GetStringValue(values[0]);
        var oldValue = TextHelpers.GetStringValue(values[1]);
        var newValue = TextHelpers.GetStringValue(values[2]);
        
        if (string.IsNullOrEmpty(oldValue))
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} oldValue cannot be empty");
            return JLioFunctionResult.Failed(currentToken);
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

        var result = ignoreCase
            ? text.Replace(oldValue, newValue, StringComparison.OrdinalIgnoreCase)
            : text.Replace(oldValue, newValue);

        return new JLioFunctionResult(true, new JValue(result));
    }
}