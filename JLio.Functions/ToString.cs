using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Functions;

public class ToString : FunctionBase
{
    public ToString()
    {
    }

    public ToString(string path)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(path)));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Arguments.Any()) return ExecuteWithArguments(currentToken, dataContext, context);

        return ExecuteWithoutArguments(currentToken);
    }

    private JLioFunctionResult ExecuteWithArguments(JToken currentToken, JToken dataContext,
        IExecutionContext context)
    {
        var value = GetArguments(Arguments, currentToken, dataContext, context).Select(i => i.ToString()).ToList();
        return new JLioFunctionResult(true, new JValue(value.FirstOrDefault()));
    }

    private JLioFunctionResult ExecuteWithoutArguments(JToken currentToken)
    {
        if (currentToken.Type == JTokenType.String) return new JLioFunctionResult(true, currentToken);

        return new JLioFunctionResult(true, new JValue(currentToken.ToString(Formatting.None)));
    }
}