using System.Collections.Generic;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NewtonsoftJSchema = Newtonsoft.Json.Schema.JSchema;

namespace JLio.Extensions.JSchema;

public class FilterBySchema : FunctionBase
{
    private const string ArrayItemsPath = "[*]";
    private const char PathDelimiter = '.';

    public FilterBySchema()
    {
    }

    public FilterBySchema(NewtonsoftJSchema schema)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(schema.ToString(),null)));
    }

    public FilterBySchema(string path)
    {
        Arguments.Add(new FunctionSupportedValue(new FixedValue(path,null)));
    }

    public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Arguments.Count == 0 || Arguments.Count > 1)
        {
            context.LogError(CoreConstants.FunctionExecution,
                $"failed: {FunctionName} requires one argument: path of the schema or the schema itself");
            return JLioFunctionResult.Failed(currentToken);
        }

        var values = GetArguments(Arguments, currentToken, dataContext, context);

        var pathsToFilter = JsonConvert.DeserializeObject<NewtonsoftJSchema>(values[0].ToString()).GetPaths();

        var currentObject = currentToken.DeepClone();
        var inputObjectPaths = GetInputPaths(currentObject);

        var pathsInSchema = pathsToFilter.Select(s => s.Path).ToList();

        foreach (var pathToRemove in GetPathsToRemove(inputObjectPaths, pathsInSchema))
        {
            var tokens = currentObject.SelectTokens(pathToRemove);
            if (tokens?.Any() == true) 
            {
                RemoveItems(currentObject, pathToRemove, context);
            }
        }

        return new JLioFunctionResult(true, currentObject);
    }

    private List<string> GetPathsToRemove(List<string> objectPaths, List<string> pathsToFilter)
    {
        return objectPaths.Except(pathsToFilter).ToList();
    }

    private List<string> GetInputPaths(JToken input)
    {
        return input.GetAllElements(f => f.Path != string.Empty)
            .Select(t => GetPath(t.Path)).ToList();
    }

    private string GetPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;
        var splittedPath = new JsonSplittedPath(path);
        var pathResult = "$.";
        foreach (var element in splittedPath.Elements)
            if (element.HasArrayIndicator)
                pathResult += $"{element.ElementName}{ArrayItemsPath}{PathDelimiter.ToString()}";
            else
                pathResult += $"{element.ElementName}{PathDelimiter.ToString()}";

        var result = pathResult.TrimEnd(PathDelimiter);
        return result;
    }

    private void RemoveItems(JToken data, string path, IExecutionContext executionContext)
    {
        var targetItems =
            executionContext.ItemsFetcher.SelectTokens(path, data);
        if (targetItems.Count == 0)
            executionContext.LogWarning(CoreConstants.CommandExecution,
                $"{path} did not retrieve any items");
        targetItems.ForEach(i =>
        {
            var success = JsonMethods.RemoveItemFromTarget(i);
            if (!success)
                executionContext.LogError(CoreConstants.CommandExecution,
                    "Removing only possible on properties or array items");
        });
    }
}
