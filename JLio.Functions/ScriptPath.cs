using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace JLio.Functions;

public class ScriptPath : FunctionBase
{
    public ScriptPath()
    {
    }

    public override string FunctionName => "path";

    public ScriptPath(string path)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(path)));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        // If no arguments, return current token path
        if (Arguments.Count == 0)
        {
            var currentPath = GetTokenPath(currentToken, context);
            return new JLioFunctionResult(true, new JValue(currentPath));
        }


        var targetPath = Arguments.First().GetStringRepresentation();
        if (string.IsNullOrEmpty(targetPath) || targetPath== "''")
        {
            var currentPath = GetTokenPath(currentToken, context);
            return new JLioFunctionResult(true, new JValue(currentPath));
        }

        // Handle @ (current item reference)
        if (targetPath.StartsWith("@"))
        {
            var currentPath = GetTokenPath(currentToken, context);
            var relativePath = targetPath.Substring(1);

            if (string.IsNullOrEmpty(relativePath))
            {
                return new JLioFunctionResult(true, new JValue(currentPath));
            }

            // Remove leading dot if present
            if (relativePath.StartsWith("."))
            {
                relativePath = relativePath.Substring(1);
            }

            var fullPath = $"{currentPath}.{relativePath}";
            return new JLioFunctionResult(true, new JValue(fullPath));
        }

        // For absolute paths, just return them as-is
        return new JLioFunctionResult(true, new JValue(targetPath));
    }

    private string GetTokenPath(JToken token, IExecutionContext context)
    {
        if (token?.Path == null || string.IsNullOrEmpty(token.Path))
        {
            return "$";
        }

        return $"$.{token.Path}";
    }
}
