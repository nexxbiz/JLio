using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class TrimStart : FunctionBase
{
    public TrimStart()
    {
    }

    public TrimStart(params string[] arguments)
    {
        foreach (var a in arguments)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));
        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count < 1 || values.Count > 2)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 1 or 2 arguments (text, [trimChars])");
            return JLioFunctionResult.Failed(currentToken);
        }

        var text = TextHelpers.GetStringValue(values[0]);
        
        string result;
        if (values.Count == 2)
        {
            var trimChars = TextHelpers.GetStringValue(values[1]);
            if (string.IsNullOrEmpty(trimChars))
            {
                result = text.TrimStart();
            }
            else
            {
                result = text.TrimStart(trimChars.ToCharArray());
            }
        }
        else
        {
            result = text.TrimStart();
        }

        return new JLioFunctionResult(true, new JValue(result));
    }
}