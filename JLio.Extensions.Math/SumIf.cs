using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JLio.Extensions.Math;

public class SumIf : FunctionBase
{
    public override string FunctionName => "sumif";
    
    public SumIf()
    {
    }

    public SumIf(params string[] arguments)
    {
        foreach (var a in arguments)

        {

            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));

        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArgumentsWithMetadata(Arguments, currentToken, dataContext, context);

        if (values.Count < 2 || values.Count > 3)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 2 or 3 arguments: range, criteria, [sum_range]");
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

        // Extract criteria string properly  
        string criteria;
        if (criteriaValue.Value.Type == JTokenType.String)
        {
            criteria = criteriaValue.Value.Value<string>();
            // Remove surrounding single quotes if present (JLio string literals)
            if (criteria.StartsWith("'") && criteria.EndsWith("'") && criteria.Length >= 2)
            {
                criteria = criteria.Substring(1, criteria.Length - 2);
            }
        }
        else
        {
            criteria = criteriaValue.Value.ToString();
        }
        
        var range = ExtractArray(rangeValue.Value);
        
        // If sum_range is provided, use it; otherwise sum from range
        List<JToken> sumRange;
        if (values.Count == 3)
        {
            var sumRangeValue = values[2];
            if (!sumRangeValue.WasFound)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} sum_range argument path not found");
                return JLioFunctionResult.Failed(currentToken);
            }
            sumRange = ExtractArray(sumRangeValue.Value);
        }
        else
        {
            sumRange = range;
        }

        double result = 0;
        int minLength = System.Math.Min(range.Count, sumRange.Count);

        for (int i = 0; i < minLength; i++)
        {
            var rangeItem = range[i];
            var sumItem = sumRange[i];

            // Extract the actual value for condition evaluation
            var testValue = ExtractValue(rangeItem);
            
            if (ConditionEvaluator.EvaluateCondition(testValue, criteria))
            {
                if (TryGetNumericValue(sumItem, out var numericValue))
                {
                    result += numericValue;
                }
            }
        }

        return new JLioFunctionResult(true, MathHelper.CreateNumericValue(result));
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
}
