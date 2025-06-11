using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

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
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count != 2)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} requires 2 arguments (base, subtract)");
            return JLioFunctionResult.Failed(currentToken);
        }

        double baseValue = 0;
        if (!TryAddTokenValue(values[0], ref baseValue, context))
        {
            return JLioFunctionResult.Failed(currentToken);
        }

        double subtractValue = 0;
        if (!TryAddTokenValue(values[1], ref subtractValue, context))
        {
            return JLioFunctionResult.Failed(currentToken);
        }

        var result = baseValue - subtractValue;
        return new JLioFunctionResult(true, new JValue(result));
    }

    private bool TryAddTokenValue(JToken token, ref double result, IExecutionContext context)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                result += token.Value<double>();
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), out var numeric):
                result += numeric;
                return true;

            case JTokenType.Null:
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
            if (!TryAddTokenValue(item, ref result, context))
            {
                return false;
            }
        }
        return true;
    }
}
