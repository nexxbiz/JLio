using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Math;

/// <summary>
/// Alias for Ceiling function - rounds up to nearest integer
/// </summary>
public class Ceil : FunctionBase
{
    public Ceil()
    {
    }

    public Ceil(params string[] arguments)
    {
        arguments.ToList().ForEach(a =>
            Arguments.Add(new FunctionSupportedValue(new FixedValue(a))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        // Delegate to Ceiling function by creating a new instance with the same arguments
        var ceiling = new Ceiling();

        // Copy arguments from this function to the ceiling function
        foreach (var argument in Arguments)
        {
            ceiling.Arguments.Add(argument);
        }

        return ceiling.Execute(currentToken, dataContext, context);
    }
}