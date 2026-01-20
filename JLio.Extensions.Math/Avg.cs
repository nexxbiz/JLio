using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Avg : FunctionBase
{
    public Avg()
    {
    }

    public Avg(params string[] arguments)
    {
        foreach (var a in arguments)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));
        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArgumentsWithMetadata(Arguments, currentToken, dataContext, context);
        double sum = 0;
        int count = 0;

        foreach (var argValue in values)
        {
            if (!TryAddTokenValue(argValue.Value, argValue.WasFound, ref sum, ref count, context))
            {
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        var result = count == 0 ? 0 : sum / count;
        return new JLioFunctionResult(true, MathHelper.CreateNumericValue(result));
    }

    private bool TryAddTokenValue(JToken token, bool wasFound, ref double sum, ref int count, IExecutionContext context)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                sum += token.Value<double>();
                count++;
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                sum += numeric;
                count++;
                return true;

            case JTokenType.Null:
                // If not found, return error
                if (!wasFound)
                {
                    context.LogError(CoreConstants.FunctionExecution,
                        $"{FunctionName} argument path not found");
                    return false;
                }
                // If found but null, treat as 0 and include in count
                count++;
                return true;

            case JTokenType.Array:
                return TryAddArrayValues((JArray)token, ref sum, ref count, context);

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values or arrays. Current type = {token.Type}");
                return false;
        }
    }

    private bool TryAddArrayValues(JArray array, ref double sum, ref int count, IExecutionContext context)
    {
        foreach (var item in array)
        {
            // Array items are always considered "found" - they exist in the array
            if (!TryAddTokenValue(item, true, ref sum, ref count, context))
            {
                return false;
            }
        }
        return true;
    }
}
