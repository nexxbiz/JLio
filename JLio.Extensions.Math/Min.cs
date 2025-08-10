using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace JLio.Extensions.Math;

public class Min : FunctionBase
{
    public Min()
    {
    }

    public Min(params string[] arguments)
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

        double minValue = double.MaxValue;
        bool hasValue = false;

        foreach (var token in values)
        {
            if (!TryFindMinValue(token, ref minValue, ref hasValue, context))
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

        return new JLioFunctionResult(true, new JValue(minValue));
    }

    private bool TryFindMinValue(JToken token, ref double minValue, ref bool hasValue, IExecutionContext context)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                var value = token.Value<double>();
                if (!hasValue || value < minValue)
                {
                    minValue = value;
                }
                hasValue = true;
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                if (!hasValue || numeric < minValue)
                {
                    minValue = numeric;
                }
                hasValue = true;
                return true;

            case JTokenType.Null:
                return true;

            case JTokenType.Array:
                return TryFindMinInArray((JArray)token, ref minValue, ref hasValue, context);

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values or arrays. Current type = {token.Type}");
                return false;
        }
    }

    private bool TryFindMinInArray(JArray array, ref double minValue, ref bool hasValue, IExecutionContext context)
    {
        foreach (var item in array)
        {
            if (!TryFindMinValue(item, ref minValue, ref hasValue, context))
            {
                return false;
            }
        }
        return true;
    }
}