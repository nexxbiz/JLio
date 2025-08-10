using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Abs : FunctionBase
{
    public Abs()
    {
    }

    public Abs(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count != 1)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires exactly one argument");
            return JLioFunctionResult.Failed(currentToken);
        }

        var token = values[0];
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
                value = 0;
                break;

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values. Current type = {token.Type}");
                return JLioFunctionResult.Failed(currentToken);
        }

        var result = System.Math.Abs(value);
        return new JLioFunctionResult(true, new JValue(result));
    }
}