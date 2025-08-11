using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class Length : FunctionBase
{
    public Length()
    {
    }

    public Length(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
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
        string stringValue = TextHelpers.GetStringValue(token);
        
        var length = stringValue.Length;
        return new JLioFunctionResult(true, new JValue(length));
    }
}