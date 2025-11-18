using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Floor : FunctionBase
{
    public Floor()
    {
    }

    public Floor(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArgumentsWithMetadata(Arguments, currentToken, dataContext, context);
        if (values.Count != 1)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires exactly one argument");
            return JLioFunctionResult.Failed(currentToken);
        }

        var argValue = values[0];
        var token = argValue.Value;
        double value;

        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                value = token.Value<double>();
                break;

            case JTokenType.String when double.TryParse(token.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                value = numeric;
                break;

            case JTokenType.Null:
                // If not found, return error
                if (!argValue.WasFound)
                {
                    context.LogError(CoreConstants.FunctionExecution,
                        $"{FunctionName} argument path not found");
                    return JLioFunctionResult.Failed(currentToken);
                }
                // If found but null, treat as 0
                value = 0;
                break;

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values. Current type = {token.Type}");
                return JLioFunctionResult.Failed(currentToken);
        }

        var result = System.Math.Floor(value);
        return new JLioFunctionResult(true, MathHelper.CreateNumericValue(result));
    }
}