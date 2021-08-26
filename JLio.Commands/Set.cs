using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands
{
    public class Set : IJLioCommand
    {
        private IExecutionOptions executionOptions;

        public Set()
        {
        }

        public Set(string path, IFunctionSupportedValue value)
        {
            Path = path;
            Value = value;
        }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("value")]
        public IFunctionSupportedValue Value { get; set; }

        public string CommandName { get; } = "set";

        public JLioExecutionResult Execute(JToken dataContext, IExecutionOptions options)
        {
            executionOptions = options;
            var validationResult = ValidateCommandInstance();
            if (!validationResult.IsValid)
            {
                validationResult.ValidationMessages.ForEach(i =>
                    options.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution, i));
                return new JLioExecutionResult(false, dataContext);
            }

            ;
            var targetPath = JsonPathMethods.SplitPath(Path);
            SetValueToObjectItems(dataContext, targetPath);
            executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"{CommandName}: completed for {targetPath.Elements.ToPathString()}");
            return new JLioExecutionResult(true, dataContext);
        }

        public ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult {IsValid = true};
            if (string.IsNullOrWhiteSpace(Path))
            {
                result.ValidationMessages.Add($"Path property for {CommandName} command is missing");
                result.IsValid = false;
            }

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