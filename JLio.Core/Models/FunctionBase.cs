using System.Collections.Generic;
using System.Linq;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models;

public abstract class FunctionBase : IFunction
{
    public Arguments Arguments = new Arguments();

    public virtual string FunctionName => GetType().Name.CamelCasing();

    public IFunction SetArguments(Arguments functionArguments)
    {
        Arguments = functionArguments;
        return this;
    }

    public string ToScript()
    {
        return
            $"{FunctionName}({string.Join(CoreConstants.ArgumentsDelimiter.ToString(), Arguments.Select(a => a.Function.ToScript()))})";
    }

    public abstract JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context);

    public static List<JToken> GetArguments(Arguments arguments, JToken currentToken, JToken dataContext,
        IExecutionContext context)
    {
        var argumentValues = new List<JToken>();
        arguments.ForEach(a => argumentValues.Add(
            a.GetValue(currentToken, dataContext, context).Data.GetJTokenValue()));
        return argumentValues;
    }

    /// <summary>
    /// Gets arguments with metadata about whether each value was found or not.
    /// This allows distinguishing between "not found" (empty result) and "found but null" (null token).
    /// </summary>
    public static List<ArgumentValue> GetArgumentsWithMetadata(Arguments arguments, JToken currentToken, JToken dataContext,
        IExecutionContext context)
    {
        var argumentValues = new List<ArgumentValue>();
        arguments.ForEach(a =>
        {
            var result = a.GetValue(currentToken, dataContext, context);
            var tokens = result.Data;
            
            // Empty collection means not found
            if (tokens.Count == 0)
            {
                argumentValues.Add(new ArgumentValue(JValue.CreateNull(), false));
            }
            else
            {
                // Found - could be null or a value
                argumentValues.Add(new ArgumentValue(tokens.GetJTokenValue(), true));
            }
        });
        return argumentValues;
    }
}

/// <summary>
/// Represents an argument value with metadata about whether it was found.
/// </summary>
public class ArgumentValue
{
    public ArgumentValue(JToken value, bool wasFound)
    {
        Value = value;
        WasFound = wasFound;
    }

    /// <summary>
    /// The token value. Will be a Null token if not found or if the value was explicitly null.
    /// </summary>
    public JToken Value { get; }

    /// <summary>
    /// True if the path was found (even if the value is null), false if the path does not exist.
    /// </summary>
    public bool WasFound { get; }
}