using JLio.Core;
using JLio.Core.Contracts;
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
            var currentPath = context.ItemsFetcher.GetPath(currentToken);
            return new JLioFunctionResult(true, new JValue(currentPath));
        }

        var targetPath = Arguments.First().GetStringRepresentation();
        if (string.IsNullOrEmpty(targetPath) || targetPath == "''")
        {
            var currentPath = context.ItemsFetcher.GetPath(currentToken);
            return new JLioFunctionResult(true, new JValue(currentPath));
        }

        // Use ItemsFetcher to resolve relative paths (including parent navigation)
        var resolvedPath = context.ItemsFetcher.ResolveRelativePath(targetPath, currentToken, dataContext);
        return new JLioFunctionResult(true, new JValue(resolvedPath));
    }
}
