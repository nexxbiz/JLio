using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions;

public class Indirect : FunctionBase
{
    public Indirect()
    {
    }

    public Indirect(string pathToPath)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(pathToPath)));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Arguments.Count != 1)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} requires exactly 1 argument (path to the path string)");
            return JLioFunctionResult.Failed(currentToken);
        }

        var pathArgument = GetArguments(Arguments, currentToken, dataContext, context).FirstOrDefault();
        if (pathArgument == null)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} argument resolved to null");
            return JLioFunctionResult.Failed(currentToken);
        }

        // Get the path string from the argument
        string indirectPath;
        if (pathArgument.Type == JTokenType.String)
        {
            indirectPath = pathArgument.Value<string>() ?? string.Empty;
        }
        else
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} requires the path reference to resolve to a string value, got {pathArgument.Type}");
            return JLioFunctionResult.Failed(currentToken);
        }

        if (string.IsNullOrWhiteSpace(indirectPath))
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} resolved path is null or empty");
            return JLioFunctionResult.Failed(currentToken);
        }

        try
        {
            // Use the indirect path to select token(s) from the data context
            var selectedToken = dataContext.SelectToken(indirectPath);
            if (selectedToken == null)
            {
                context.LogWarning(CoreConstants.FunctionExecution,
                    $"{FunctionName}: indirect path '{indirectPath}' did not match any tokens");
                return JLioFunctionResult.Failed(currentToken);
            }

            return new JLioFunctionResult(true, selectedToken);
        }
        catch (System.Exception ex)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} invalid JSONPath '{indirectPath}': {ex.Message}");
            return JLioFunctionResult.Failed(currentToken);
        }
    }
}