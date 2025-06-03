using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions;

public class NewGuid : FunctionBase
{
    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        return new JLioFunctionResult(true, new JValue(Guid.NewGuid().ToString()));
    }
}