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
            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                result += token.Value<double>();
            }
            else if (token.Type == JTokenType.String && double.TryParse(token.Value<string>(), out var numeric))
            {
                result += numeric;
            }
            else
            {
                context.LogError(CoreConstants.FunctionExecution,
                    $"{FunctionName} can only handle numeric values. Current type = {token.Type}");
                return JLioFunctionResult.Failed(currentToken);
            }
        }
        return new JLioFunctionResult(true, new JValue(result));
    }
}
