using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JLio.Extensions.Math;

public class AverageIf : FunctionBase
{
    public override string FunctionName => "averageif";
    
    public AverageIf()
    {
    }

    public AverageIf(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArgumentsWithMetadata(Arguments, currentToken, dataContext, context);

        if (values.Count < 2 || values.Count > 3)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 2 or 3 arguments: range, criteria, [average_range]");
            return JLioFunctionResult.Failed(currentToken);
        }

        var rangeValue = values[0];
        var criteriaValue = values[1];

        if (!rangeValue.WasFound)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} range argument path not found");
            return JLioFunctionResult.Failed(currentToken);
        }

        if (!criteriaValue.WasFound)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} criteria argument path not found");
            return JLioFunctionResult.Failed(currentToken);
        }

        var criteria = ExtractCriteriaString(criteriaValue.Value);
        var range = ExtractArray(rangeValue.Value);
        
        // If average_range is provided, use it; otherwise average from range
        List<JToken> averageRange;
        if (values.Count == 3)
        {
            var averageRangeValue = values[2];
            if (!averageRangeValue.WasFound)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} average_range argument path not found");
                return JLioFunctionResult.Failed(currentToken);
            }
            averageRange = ExtractArray(averageRangeValue.Value);
        }
        else
        {
            averageRange = range;
        }

        double sum = 0;
        int count = 0;
        int minLength = System.Math.Min(range.Count, averageRange.Count);

        for (int i = 0; i < minLength; i++)
        {
            var rangeItem = range[i];
            var avgItem = averageRange[i];

            var testValue = ExtractValue(rangeItem);
            
            if (ConditionEvaluator.EvaluateCondition(testValue, criteria))
            {
                if (TryGetNumericValue(avgItem, out var numericValue))
                {
                    sum += numericValue;
                    count++;
                }
            }
        }

        var result = count == 0 ? 0 : sum / count;
        return new JLioFunctionResult(true, new JValue(result));
    }

    private List<JToken> ExtractArray(JToken token)
    {
        if (token.Type == JTokenType.Array)
        {
            return token.ToList();
        }
        return new List<JToken> { token };
    }

    private object ExtractValue(JToken token)
    {
        return token.Type switch
        {
            JTokenType.Integer => token.Value<long>(),
            JTokenType.Float => token.Value<double>(),
            JTokenType.String => token.Value<string>(),
            JTokenType.Boolean => token.Value<bool>(),
            JTokenType.Null => null,
            _ => token.ToString()
        };
    }

    private bool TryGetNumericValue(JToken token, out double value)
    {
        value = 0;

        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                value = token.Value<double>();
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), NumberStyles.Float, CultureInfo.InvariantCulture, out var numeric):
                value = numeric;
                return true;

            default:
                return false;
        }
    }
    private string ExtractCriteriaString(JToken token)
    {
        return token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
    }
}
