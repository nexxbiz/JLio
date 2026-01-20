using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JLio.Extensions.Math;

public class MinIfs : FunctionBase
{
    public override string FunctionName => "minifs";
    
    public MinIfs()
    {
    }

    public MinIfs(params string[] arguments)
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
                $"{FunctionName} requires an odd number of arguments: min_range, criteria_range1, criteria1, [criteria_range2, criteria2], ...");
            return JLioFunctionResult.Failed(currentToken);
        }

        var minRangeValue = values[0];
        if (!minRangeValue.WasFound)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} min_range argument path not found");
            return JLioFunctionResult.Failed(currentToken);
        }

        var minRange = ExtractArray(minRangeValue.Value);

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

        double minValue = double.MaxValue;
        bool hasValue = false;
        
        for (int i = 0; i < minRange.Count; i++)
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
                if (TryGetNumericValue(minRange[i], out var numericValue))
                {
                    if (!hasValue || numericValue < minValue)
                    {
                        minValue = numericValue;
                    }
                    hasValue = true;
                }
            }
        }

        if (!hasValue)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} found no valid numeric values matching criteria");
            return JLioFunctionResult.Failed(currentToken);
        }

        return new JLioFunctionResult(true, MathHelper.CreateNumericValue(minValue));
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
