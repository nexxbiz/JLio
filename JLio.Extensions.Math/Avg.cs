using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Math;

public class Avg : FunctionBase
{
    public Avg()
    {
    }

    public Avg(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        double sum = 0;
        int count = 0;

        foreach (var token in values)
        {
            if (!TryAddTokenValue(token, ref sum, ref count, context))
            {
                return JLioFunctionResult.Failed(currentToken);
            }
        }

        var result = count == 0 ? 0 : sum / count;
        return new JLioFunctionResult(true, new JValue(result));
    }

    private bool TryAddTokenValue(JToken token, ref double sum, ref int count, IExecutionContext context)
    {
        switch (token.Type)
        {
            case JTokenType.Integer:
            case JTokenType.Float:
                sum += token.Value<double>();
                count++;
                return true;

            case JTokenType.String when double.TryParse(token.Value<string>(), out var numeric):
                sum += numeric;
                count++;
                return true;

            case JTokenType.Array:
                return TryAddArrayValues((JArray)token, ref sum, ref count, context);

            default:
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values or arrays. Current type = {token.Type}");
                return false;
        }
    }

    private bool TryAddArrayValues(JArray array, ref double sum, ref int count, IExecutionContext context)
    {
        foreach (var item in array)
        {
            if (!TryAddTokenValue(item, ref sum, ref count, context))
            {
                return false;
            }
        }
        return true;
    }
}
