using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions;

public class Sum : FunctionBase
{
    public Sum()
    {
    }

    public Sum(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        double result = 0;

        foreach (var token in values)
        {
            if (!TryAddTokenValue(token, ref result, context))
            {
                return JLioFunctionResult.Failed(currentToken);
            }
        }

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
