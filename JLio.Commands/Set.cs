using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extentions;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace JLio.Commands
{
    public class Set : IJLioCommand
    {
        private IJLioExecutionOptions executionOptions;

        public string CommandName { get; } = "set";

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("value")]
        public IJLioFunctionSupportedValue Value { get; set; }

        public JLioExecutionResult Execute(JToken dataContext, IJLioExecutionOptions options)
        {
            executionOptions = options;

            if (!ValidateCommandInstance()) return new JLioExecutionResult(false, dataContext);
            var targetPath = JsonPathMethods.SplitPath(Path);
            JsonMethods.CheckOrCreateParentPath(dataContext, targetPath, options.ItemsFetcher, options.Logger);
            SetValueToObjectItems(dataContext, targetPath);
            executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"{CommandName}: completed for {targetPath.Elements.ToPathString()}");
            return new JLioExecutionResult(true, dataContext);
        }

        public bool ValidateCommandInstance()
        {
            var result = true;
            if (string.IsNullOrWhiteSpace(Path))
            {
                executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                    $"Path property for {CommandName} command is missing");
                result = false;
            }

            if (result == false)
                executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution, "Command not executed");

            return result;
        }

        private void SetValueToObjectItems(JToken dataContext, JsonSplittedPath targetPath)
        {
            var path = targetPath.Elements
                .Take(targetPath.Elements.Count() - 1)
                .ToPathString();
            if (targetPath.IsSearchingForObjectsByName)
                path = targetPath.Elements.ToPathString();
            var targetItems =
                executionOptions.ItemsFetcher.SelectTokens(path, dataContext);
            targetItems.ForEach(i => SetValueToTarget(targetPath.LastName, i, dataContext));
        }

        private void SetValueToTarget(string propertyName, JToken jToken, JToken dataContext)
        {
            switch (jToken)
            {
                case JObject o:
                    if (!o.ContainsKey(propertyName))
                    {
                        executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                            $"Property {propertyName} does not exists on {o.Path}. {CommandName} function not applied.");
                        return;
                    }

                    ReplaceCurrentValueWithNew(propertyName, o, dataContext);
                    break;
                case JArray a:
                    executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                        $"can't set value on a array on {a.Path}. {CommandName} functionality not applied.");
                    break;
            }
        }

        private void ReplaceCurrentValueWithNew(string propertyName, JObject o, JToken dataContext)
        {
                o[propertyName] = Value.GetValue(o[propertyName], dataContext, executionOptions);

            executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"Property {propertyName} on {o.Path} value has been set.");
        }

    }
}





