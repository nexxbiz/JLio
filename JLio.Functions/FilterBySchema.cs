using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
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
            Arguments.Add(new FunctionSupportedValue(new FixedValue(JSchema.Parse($"\"{path}\""))));
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
            
            var inputObjectPaths = GetInputPaths(currentToken);

            var pathsInSchema = pathsToFilter.Select(s => s.Path).ToList();
            
            foreach (var pathToRemove in GetPathsToRemove(inputObjectPaths, pathsInSchema))
            {
                var token = currentToken.SelectToken(pathToRemove);
                if (token != null)
                {
                    RemoveItems(currentToken, pathToRemove, context);
                }
            }
            return new JLioFunctionResult(true, currentToken);
        }

        private List<string> GetPathsToRemove(List<string> objectPaths, List<string> pathsToFilter)
        {
            return  objectPaths.Except(pathsToFilter).ToList();
        }

        private List<string> GetInputPaths(JToken input)
        {
            var paths = new List<string>();
            var tokens = JsonMethods.GetAllElements(input);
            foreach (var jsonToken in tokens)
            {
                if (jsonToken.Path == string.Empty)
                {
                    continue;
                }
                paths.Add(GetPath(jsonToken.Path));
            }

            return paths;
        }

        private string GetPath(string path)
        {
            var result = $"$.{path}";
            if (result.Contains("["))
            {
                result = Regex.Replace(path, "\\[(.*?)\\]", "[*]");
            }
            return  result;
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