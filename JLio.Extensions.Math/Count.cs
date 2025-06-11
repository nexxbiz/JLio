using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Math;

public class Count : FunctionBase
{
    public Count()
    {
    }

    public Count(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        int count = 0;

        foreach (var token in values)
        {
            AddToken(token, ref count);
        }

        return new JLioFunctionResult(true, new JValue(count));
    }

    private void AddToken(JToken token, ref int count)
    {
        switch (token.Type)
        {
            case JTokenType.Array:
                count += token.Count();
                break;
            case JTokenType.Object:
                count += ((JObject)token).Properties().Count();
                break;
            default:
                count++;
                break;
        }
    }
}
