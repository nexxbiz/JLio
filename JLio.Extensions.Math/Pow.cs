using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Pow : FunctionBase
{
    public Pow()
    {
    }

    public Pow(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count != 2)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires exactly 2 arguments (base, exponent)");
            return JLioFunctionResult.Failed(currentToken);
        }

        // Get base value
        double baseValue;
        var baseToken = values[0];
        switch (baseToken.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                baseValue = baseToken.Value<double>();
                break;

            case JTokenType.String when double.TryParse(baseToken.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                baseValue = numeric;
                break;

            case JTokenType.Null:
                baseValue = 0;
                break;

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} base argument must be numeric. Current type = {baseToken.Type}");
                return JLioFunctionResult.Failed(currentToken);
        }

        // Get exponent value
        double exponent;
        var exponentToken = values[1];
        switch (exponentToken.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                exponent = exponentToken.Value<double>();
                break;

            case JTokenType.String when double.TryParse(exponentToken.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                exponent = numeric;
                break;

            case JTokenType.Null:
                exponent = 0;
                break;

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} exponent argument must be numeric. Current type = {exponentToken.Type}");
                return JLioFunctionResult.Failed(currentToken);
        }

        var result = System.Math.Pow(baseValue, exponent);

        // Check for invalid results
        if (double.IsNaN(result) || double.IsInfinity(result))
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} calculation resulted in an invalid number (base={baseValue}, exponent={exponent})");
            return JLioFunctionResult.Failed(currentToken);
        }

        return new JLioFunctionResult(true, new JValue(result));
    }
}