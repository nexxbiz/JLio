using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace JLio.Extensions.Math;

public class CountIf : FunctionBase
{
    public override string FunctionName => "countif";
    
    public CountIf()
    {
    }

    public CountIf(params string[] arguments)
    {
        foreach (var a in arguments)

        {

            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));

        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArgumentsWithMetadata(Arguments, currentToken, dataContext, context);

        if (values.Count != 2)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires 2 arguments: range, criteria");
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
        
        int count = 0;

        foreach (var item in range)
        {
            var testValue = ExtractValue(item);
            
            if (ConditionEvaluator.EvaluateCondition(testValue, criteria))
            {
                count++;
            }
        }

        return new JLioFunctionResult(true, new JValue(count));
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
