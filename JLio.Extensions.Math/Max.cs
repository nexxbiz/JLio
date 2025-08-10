using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Max : FunctionBase
{
    public Max()
    {
    }

    public Max(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count == 0)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires at least one argument");
            return JLioFunctionResult.Failed(currentToken);
        }

        double maxValue = double.MinValue;
        bool hasValue = false;

        foreach (var token in values)
        {
            if (!TryFindMaxValue(token, ref maxValue, ref hasValue, context))
            {
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        if (!hasValue)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} found no valid numeric values");
            return JLioFunctionResult.Failed(currentToken);
        }

        return new JLioFunctionResult(true, new JValue(maxValue));
    }

    private bool TryFindMaxValue(JToken token, ref double maxValue, ref bool hasValue, IExecutionContext context)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                var value = token.Value<double>();
                if (!hasValue || value > maxValue)
                {
                    maxValue = value;
                }
                hasValue = true;
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                if (!hasValue || numeric > maxValue)
                {
                    maxValue = numeric;
                }
                hasValue = true;
                return true;

            case JTokenType.Null:
                return true;

            case JTokenType.Array:
                return TryFindMaxInArray((JArray)token, ref maxValue, ref hasValue, context);

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values or arrays. Current type = {token.Type}");
                return false;
        }
    }

    private bool TryFindMaxInArray(JArray array, ref double maxValue, ref bool hasValue, IExecutionContext context)
    {
        foreach (var item in array)
        {
            if (!TryFindMaxValue(item, ref maxValue, ref hasValue, context))
            {
                return false;
            }
        }
        return true;
    }
}