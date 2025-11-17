using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Subtract : FunctionBase
{
    public Subtract()
    {
    }

    public Subtract(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArgumentsWithMetadata(Arguments, currentToken, dataContext, context);
        if (values.Count != 2)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} requires 2 arguments (base, subtract)");
            return JLioFunctionResult.Failed(currentToken);
        }

        double baseValue = 0;
        if (!TryAddTokenValue(values[0].Value, values[0].WasFound, ref baseValue, context))
        {
            return JLioFunctionResult.Failed(currentToken);
        }

        double subtractValue = 0;
        if (!TryAddTokenValue(values[1].Value, values[1].WasFound, ref subtractValue, context))
        {
            return JLioFunctionResult.Failed(currentToken);
        }

        var result = baseValue - subtractValue;
        return new JLioFunctionResult(true, new JValue(result));
    }

    private bool TryAddTokenValue(JToken token, bool wasFound, ref double result, IExecutionContext context)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                result += token.Value<double>();
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                result += numeric;
                return true;

            case JTokenType.Null:
                // If not found, return error
                if (!wasFound)
                {
                    context.LogError(CoreConstants.FunctionExecution,
                        $"{FunctionName} argument path not found");
                    return false;
                }
                // If found but null, treat as 0 (do nothing, result += 0)
                return true;

            case JTokenType.Array:
                return TryAddArrayValues((JArray)token, ref result, context);

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values or arrays. Current type = {token.Type}");
                return false;
        }
    }

    private bool TryAddArrayValues(JArray array, ref double result, IExecutionContext context)
    {
        foreach (var item in array)
        {
            // Array items are always considered "found" - they exist in the array
            if (!TryAddTokenValue(item, true, ref result, context))
            {
                return false;
            }
        }
        return true;
    }
}
