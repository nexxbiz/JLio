using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

/// <summary>
/// Calculates the modulo (remainder) of dividend divided by divisor.
/// </summary>
public class Modulo : FunctionBase
{
    public Modulo()
    {
    }

    public Modulo(params string[] arguments)
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
                $"{FunctionName} requires exactly 2 arguments (dividend, divisor)");
            return JLioFunctionResult.Failed(currentToken);
        }

        double dividend = 0;
        if (!TryGetNumericValue(values[0].Value, values[0].WasFound, ref dividend, context))
        {
            return JLioFunctionResult.Failed(currentToken);
        }

        double divisor = 0;
        if (!TryGetNumericValue(values[1].Value, values[1].WasFound, ref divisor, context))
        {
            return JLioFunctionResult.Failed(currentToken);
        }

        // Check for division by zero
        if (divisor == 0)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} divisor cannot be zero");
            return JLioFunctionResult.Failed(currentToken);
        }

        var result = dividend % divisor;
        return new JLioFunctionResult(true, MathHelper.CreateNumericValue(result));
    }

    private bool TryGetNumericValue(JToken token, bool wasFound, ref double result, IExecutionContext context)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                result = token.Value<double>();
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                result = numeric;
                return true;

            case JTokenType.Null:
                // If not found, return error
                if (!wasFound)
                {
                    context.LogError(CoreConstants.FunctionExecution,
                        $"{FunctionName} argument path not found");
                    return false;
                }
                // If found but null, treat as 0
                result = 0;
                return true;

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values. Current type = {token.Type}");
                return false;
        }
    }
}
