using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class Trim : FunctionBase
{
    public Trim()
    {
    }

    public Trim(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
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
                result = text.Trim();
            }
            else
            {
                result = text.Trim(trimChars.ToCharArray());
            }
        }
        else
        {
            result = text.Trim();
        }

        return new JLioFunctionResult(true, new JValue(result));
    }
}