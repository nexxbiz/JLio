using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class Split : FunctionBase
{
    public Split()
    {
    }

    public Split(params string[] arguments)
    {
        foreach (var a in arguments)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));
        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count < 2 || values.Count > 4)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 2 to 4 arguments (text, separator, [maxSplits], [removeEmpty])");
            return JLioFunctionResult.Failed(currentToken);
        }

        var text = TextHelpers.GetStringValue(values[0]);
        var separator = TextHelpers.GetStringValue(values[1]);
        
        var maxSplits = int.MaxValue;
        if (values.Count >= 3)
        {
            if (!TextHelpers.TryGetIntegerValue(values[2], out maxSplits) || maxSplits < 0)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} maxSplits must be a non-negative integer");
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        var removeEmpty = false;
        if (values.Count == 4)
        {
            if (!TextHelpers.TryGetBooleanValue(values[3], out removeEmpty))
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} removeEmpty must be a valid boolean");
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        string[] parts;
        var splitOptions = removeEmpty ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None;
        
        if (string.IsNullOrEmpty(separator))
        {
            // Split by whitespace
            parts = text.Split((char[]?)null, splitOptions);
        }
        else
        {
            if (maxSplits == int.MaxValue)
            {
                parts = text.Split(new string[] { separator }, splitOptions);
            }
            else
            {
                parts = text.Split(new string[] { separator }, maxSplits + 1, splitOptions); // +1 because Split uses count, not max splits
            }
        }

        var resultArray = new JArray();
        foreach (var part in parts)
        {
            resultArray.Add(part);
        }

        return new JLioFunctionResult(true, resultArray);
    }
}