using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JLio.Extensions.Math;

public class SumIfs : FunctionBase
{
    public override string FunctionName => "sumifs";
    
    public SumIfs()
    {
    }

    public SumIfs(params string[] arguments)
    {
        foreach (var a in arguments)

        {

            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));

        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArgumentsWithMetadata(Arguments, currentToken, dataContext, context);

        if (values.Count < 3 || values.Count % 2 == 0)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires an odd number of arguments: sum_range, criteria_range1, criteria1, [criteria_range2, criteria2], ...");
            return JLioFunctionResult.Failed(currentToken);
        }

        var sumRangeValue = values[0];
        if (!sumRangeValue.WasFound)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} sum_range argument path not found");
            return JLioFunctionResult.Failed(currentToken);
        }

        var sumRange = ExtractArray(sumRangeValue.Value);

        // Extract criteria ranges and criteria
        var criteriaRanges = new List<List<JToken>>();
        var criteriaList = new List<string>();

        for (int i = 1; i < values.Count; i += 2)
        {
            var criteriaRangeValue = values[i];
            var criteriaValue = values[i + 1];

            if (!criteriaRangeValue.WasFound)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} criteria_range argument at position {i} path not found");
                return JLioFunctionResult.Failed(currentToken);
            }

            if (!criteriaValue.WasFound)
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} criteria argument at position {i + 1} path not found");
                return JLioFunctionResult.Failed(currentToken);
            }

            criteriaRanges.Add(ExtractArray(criteriaRangeValue.Value));
            criteriaList.Add(ExtractCriteriaString(criteriaValue.Value));
        }

        double result = 0;
        
        for (int i = 0; i < sumRange.Count; i++)
        {
            bool allCriteriaMatch = true;

            // Check all criteria
            for (int c = 0; c < criteriaRanges.Count; c++)
            {
                if (i >= criteriaRanges[c].Count)
                {
                    allCriteriaMatch = false;
                    break;
                }

                var testValue = ExtractValue(criteriaRanges[c][i]);
                if (!ConditionEvaluator.EvaluateCondition(testValue, criteriaList[c]))
                {
                    allCriteriaMatch = false;
                    break;
                }
            }

            if (allCriteriaMatch)
            {
                if (TryGetNumericValue(sumRange[i], out var numericValue))
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

    private string ExtractCriteriaString(JToken token)
    {
        var str = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
        // Remove surrounding single quotes if present (JLio string literals)
        if (str.StartsWith("'") && str.EndsWith("'") && str.Length >= 2)
        {
            str = str.Substring(1, str.Length - 2);
        }
        return str;
    }
}
