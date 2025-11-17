using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Round : FunctionBase
{
    public Round()
    {
    }

    public Round(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArgumentsWithMetadata(Arguments, currentToken, dataContext, context);
        if (values.Count < 1 || values.Count > 2)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 1 or 2 arguments (value, [decimals])");
            return JLioFunctionResult.Failed(currentToken);
        }

        // Get the value to round
        double value;
        var valueArg = values[0];
        var valueToken = valueArg.Value;
        switch (valueToken.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                value = valueToken.Value<double>();
                break;

            case JTokenType.String when double.TryParse(valueToken.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                value = numeric;
                break;

            case JTokenType.Null:
                // If not found, return error
                if (!valueArg.WasFound)
                {
                    context.LogError(CoreConstants.FunctionExecution,
                        $"{FunctionName} value argument path not found");
                    return JLioFunctionResult.Failed(currentToken);
                }
                // If found but null, treat as 0
                value = 0;
                break;

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} first argument must be numeric. Current type = {valueToken.Type}");
                return JLioFunctionResult.Failed(currentToken);
        }

        // Get decimal places (default to 0)
        int decimals = 0;
        if (values.Count == 2)
        {
            var decimalsArg = values[1];
            var decimalsToken = decimalsArg.Value;
            switch (decimalsToken.Type)
            {
                case JTokenType.Integer:
                    decimals = decimalsToken.Value<int>();
                    break;

                case JTokenType.String when int.TryParse(decimalsToken.Value<string>(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var intVal):
                    decimals = intVal;
                    break;

                case JTokenType.Null:
                    // If not found, return error
                    if (!decimalsArg.WasFound)
                    {
                        context.LogError(CoreConstants.FunctionExecution,
                            $"{FunctionName} decimals argument path not found");
                        return JLioFunctionResult.Failed(currentToken);
                    }
                    // If found but null, treat as 0
                    decimals = 0;
                    break;

                default:
                    context.LogError(CoreConstants.FunctionExecution,
                        $"{FunctionName} second argument (decimals) must be an integer. Current type = {decimalsToken.Type}");
                    return JLioFunctionResult.Failed(currentToken);
            }
        }

        if (decimals < 0 || decimals > 15)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} decimal places must be between 0 and 15. Current value = {decimals}");
            return JLioFunctionResult.Failed(currentToken);
        }

        var result = System.Math.Round(value, decimals);
        return new JLioFunctionResult(true, new JValue(result));
    }
}