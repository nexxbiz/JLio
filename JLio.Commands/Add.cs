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
    public class Add : IJLioCommand
    {
        private IJLioExecutionOptions executionOptions;
        //private JToken valueResult;

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("value")]
        public IJLioFunctionSupportedValue Value { get; set; }

        public string CommandName { get; } = "add";

        public JLioExecutionResult Execute(JToken dataContext, IJLioExecutionOptions options)
        {
            executionOptions = options;
            if (!ValidateCommandInstance()) return new JLioExecutionResult(false, dataContext);

            var targetPath = JsonPathMethods.SplitPath(Path);
            JsonMethods.CheckOrCreateParentPath(dataContext, targetPath, options.ItemsFetcher, options.Logger);
            AddToObjectItems(dataContext, options.ItemsFetcher, targetPath);
            options.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
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

        private void AddToObjectItems(JToken dataContext, IItemsFetcher dataFetcher, JsonSplittedPath targetPath)
        {
            var path = targetPath.Elements
                .Take(targetPath.Elements.Count() - 1)
                .ToPathString();
            if (targetPath.IsSearchingForObjectsByName)
                path = targetPath.Elements.ToPathString();
            var targetItems =
                dataFetcher.SelectTokens(path, dataContext);
            targetItems.ForEach(i => AddValueToTarget(targetPath.LastName, i, dataContext));
        }

        private void AddValueToTarget(string propertyName, JToken jToken, JToken dataContext)
        {
            switch (jToken)
            {
                case JObject o:
                    if (JsonMethods.IsPropertyOfTypeArray(propertyName, o))
                    {
                        AddToArray((JArray) o[propertyName], dataContext);
                        return;
                    }
                    else if (o.ContainsKey(propertyName))
                    {
                        executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                            $"Property {propertyName} already exists on {o.Path}. {CommandName} function not applied");
                        return;
                    }

                    AddProperty(propertyName, o, dataContext);
                    break;
                case JArray a:
                    AddToArray(a, dataContext);
                    break;
            }
        }

        private void AddProperty(string propertyName, JObject o, JToken dataContext)
        {
            o.Add(propertyName, Value.GetValue(o, dataContext, executionOptions));
            executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"Property {propertyName} added to object: {o.Path}");
        }

        private void AddToArray(JArray jArray, JToken dataContext)
        {
            jArray.Add(Value.GetValue(jArray, dataContext, executionOptions));
            executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"Value added to array: {jArray.Path}");
        }
    }
}