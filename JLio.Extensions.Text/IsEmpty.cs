using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class IsEmpty : FunctionBase
{
    public IsEmpty()
    {
    }

    public IsEmpty(params string[] arguments)
    {
        foreach (var a in arguments)

        {

            Arguments.Add(new FunctionSupportedValue(new FixedValue(a)));

        }
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count != 1)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires exactly one argument");
            return JLioFunctionResult.Failed(currentToken);
        }

        var token = values[0];
        bool isEmpty = token.Type switch
        {
            JTokenType.String => string.IsNullOrEmpty(token.Value<string>()),
            JTokenType.Null => true,
            JTokenType.Array => !((JArray)token).Any(),
            JTokenType.Object => !((JObject)token).Properties().Any(),
            _ => false
        };

        return new JLioFunctionResult(true, new JValue(isEmpty));
    }
}