using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions;

public class Fetch : FunctionBase
{
    public Fetch()
    {
    }

    public Fetch(string path)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(new JValue(path))));
    }

    public Fetch(string path, string defaultValue)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(new JValue(path))));
        Arguments.Add(new FunctionSupportedValue(new FixedValue(new JValue(defaultValue))));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Arguments.Count == 0)
        {
            return new JLioFunctionResult(false, JValue.CreateNull());
        }

        // Get the first argument (path) evaluation result
        var firstArgument = Arguments.FirstOrDefault();
        if (firstArgument == null)
        {
            return new JLioFunctionResult(false, JValue.CreateNull());
        }

        var pathResult = firstArgument.GetValue(currentToken, dataContext, context);
        if (!pathResult.Success)
        {
            // Path evaluation failed, return default if available
            if (Arguments.Count > 1)
            {
                var defaultArgument = Arguments[1];
                var defaultResult = defaultArgument.GetValue(currentToken, dataContext, context);
                return new JLioFunctionResult(true, GetParsedDefaultValue(defaultResult.Data.GetJTokenValue()));
            }
            return new JLioFunctionResult(false, JValue.CreateNull());
        }

        // Check if the path evaluation found any tokens
        var selectedTokens = pathResult.Data;
        if (selectedTokens is SelectedTokens tokens)
        {
            // If no tokens found, path doesn't exist - return default if available
            if (tokens.Count == 0)
            {
                if (Arguments.Count > 1)
                {
                    var defaultArgument = Arguments[1];
                    var defaultResult = defaultArgument.GetValue(currentToken, dataContext, context);
                    return new JLioFunctionResult(true, GetParsedDefaultValue(defaultResult.Data.GetJTokenValue()));
                }
                // Return success with null for missing path (original behavior)
                return new JLioFunctionResult(true, JValue.CreateNull());
            }
            
            // Path exists, return the first token (even if it's null)
            return new JLioFunctionResult(true, tokens.GetJTokenValue());
        }

        // Fallback to original behavior for backward compatibility
        return new JLioFunctionResult(true, selectedTokens.GetJTokenValue());
    }

    private JToken GetParsedDefaultValue(JToken defaultValue)
    {
        // If the default value is a string, try to parse it as JSON to preserve types
        if (defaultValue != null && defaultValue.Type == JTokenType.String)
        {
            var stringValue = defaultValue.Value<string>();
            
            // Try to parse as JSON first to handle complex literals like arrays, objects, booleans, numbers, null
            try
            {
                var parsed = JToken.Parse(stringValue);
                return parsed;
            }
            catch
            {
                // If parsing fails, return as string (it's a literal string value)
                return defaultValue;
            }
        }
        
        // Return as-is if it's not a string or is null
        return defaultValue;
    }
}