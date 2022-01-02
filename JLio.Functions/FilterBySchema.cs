using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using JLio.Core.Models.Path;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace JLio.Functions
{
    public class FilterBySchema : FunctionBase
    {
        public FilterBySchema()
        {
        }

        public FilterBySchema(JSchema schema)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(schema)));
        }

        public FilterBySchema(string path)
        {
            Arguments.Add(new FunctionSupportedValue(new FixedValue(path)));
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

            var pathsToFilter = values[0].ToObject<JSchema>().GetPaths();

            var currentObject = currentToken.DeepClone();
            var inputObjectPaths = GetInputPaths(currentObject);

            var pathsInSchema = pathsToFilter.Select(s => s.Path).ToList();

            foreach (var pathToRemove in GetPathsToRemove(inputObjectPaths, pathsInSchema))
            {
                var token = currentObject.SelectToken(pathToRemove);
                if (token != null)
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
            {
                if (element.HasArrayIndicator)
                {
                    pathResult += $"{element.ElementName}[*].";
                }
                else
                {
                    pathResult += $"{element.ElementName}.";
                }
            }

            var result = pathResult.TrimEnd('.');
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
}