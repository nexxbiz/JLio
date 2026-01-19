using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

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

        // Get dividend value
        double dividend;
        var dividendArg = values[0];
        var dividendToken = dividendArg.Value;
        switch (dividendToken.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                dividend = dividendToken.Value<double>();
                break;

            case JTokenType.String when double.TryParse(dividendToken.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                dividend = numeric;
                break;

            case JTokenType.Null:
                // If not found, return error
                if (!dividendArg.WasFound)
                {
                    context.LogError(CoreConstants.FunctionExecution,
                        $"{FunctionName} dividend argument path not found");
                    return JLioFunctionResult.Failed(currentToken);
                }
                // If found but null, treat as 0
                dividend = 0;
                break;

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} dividend argument must be numeric. Current type = {dividendToken.Type}");
                return JLioFunctionResult.Failed(currentToken);
        }

        // Get divisor value
        double divisor;
        var divisorArg = values[1];
        var divisorToken = divisorArg.Value;
        switch (divisorToken.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                divisor = divisorToken.Value<double>();
                break;

            case JTokenType.String when double.TryParse(divisorToken.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                divisor = numeric;
                break;

            case JTokenType.Null:
                // If not found, return error
                if (!divisorArg.WasFound)
                {
                    context.LogError(CoreConstants.FunctionExecution,
                        $"{FunctionName} divisor argument path not found");
                    return JLioFunctionResult.Failed(currentToken);
                }
                // If found but null, treat as 0
                divisor = 0;
                break;

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} divisor argument must be numeric. Current type = {divisorToken.Type}");
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

        // Check for invalid results
        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} calculation resulted in an invalid number (dividend={dividend}, divisor={divisor})");
            return JLioFunctionResult.Failed(currentToken);
        }

        return new JLioFunctionResult(true, MathHelper.CreateNumericValue(result));
    }
}
