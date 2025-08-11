using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

public class Join : FunctionBase
{
    public Join()
    {
    }

    public Join(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var values = GetArguments(Arguments, currentToken, dataContext, context);
        if (values.Count < 2)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"{FunctionName} requires at least 2 arguments (separator, array or multiple values)");
            return JLioFunctionResult.Failed(currentToken);
        }

        var separator = TextHelpers.GetStringValue(values[0]);
        var items = new List<string>();

        // Check if second argument is an array
        if (values.Count == 2 && values[1].Type == JTokenType.Array)
        {
            var array = (JArray)values[1];
            foreach (var item in array)
            {
                items.Add(TextHelpers.GetStringValue(item));
            }
        }
        else
        {
            // Use remaining arguments as items to join
            for (int i = 1; i < values.Count; i++)
            {
                items.Add(TextHelpers.GetStringValue(values[i]));
            }
        }

        var result = string.Join(separator, items);
        return new JLioFunctionResult(true, new JValue(result));
    }
}