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
        private FunctionAnalysis functionAnalysis;
        private IJLioExecutionOptions options;

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("value")]
        public JToken Value { get; set; }

        public string CommandName { get; } = "add";

        public JLioExecutionResult Execute(JToken data, IJLioExecutionOptions options)
        {
            this.options = options;
            if (!ValidateCommandInstance()) return new JLioExecutionResult(false, data);
            var targetPath = JsonPathMethods.SplitPath(Path);
            JsonMethods.CheckOrCreateParentPath(data, targetPath, options.ItemsFetcher, options.Logger);
            functionAnalysis = EvaluateForFunction(Value);
            AddToObjectItems(data, options.ItemsFetcher, targetPath);
            options.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"{CommandName}: completed for {targetPath.Elements.ToPathString()}");
            return new JLioExecutionResult(true, data);
        }

        private FunctionAnalysis EvaluateForFunction(JToken value)
        {
            return EvaluateForFunction(value.ToString());
        }

        private FunctionAnalysis EvaluateForFunction(string value)
        {
            return FunctionAnalysis.DoAnalysis(value);
        }

        public bool ValidateCommandInstance()
        {
            var result = true;
            if (string.IsNullOrWhiteSpace(Path))
            {
                options.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                    $"Path property for {CommandName} command is missing");
                result = false;
            }

            if (Value.Type == JTokenType.None || Value.Type == JTokenType.Null)
            {
                options.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                    $"Value for {CommandName} command is missing");
                result = false;
            }

            if (result == false)
                options.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution, "Command not executed");

            return result;
        }

        private void AddToObjectItems(JToken data, IItemsFetcher dataFetcher, JsonSplittedPath targetPath)
        {
            var path = targetPath.Elements
                .Take(targetPath.Elements.Count() - 1)
                .ToPathString();
            if (targetPath.IsSearchingForObjectsByName)
                path = targetPath.Elements.ToPathString();
            var targetItems =
                dataFetcher.SelectTokens(path, data);
            targetItems.ForEach(i => AddValueToTarget(targetPath.LastName, i));
        }

        private void AddValueToTarget(string propertyName, JToken jToken)
        {
            switch (jToken)
            {
                case JObject o:
                    if (JsonMethods.IsPropertyOfTypeArray(propertyName, o))
                    {
                        AddToArray((JArray) o[propertyName]);
                        return;
                    }
                    else if (o.ContainsKey(propertyName))
                    {
                        options.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                            $"Property {propertyName} already exists on {o.Path}. {CommandName} function not applied");
                        return;
                    }

                    AddProperty(propertyName, o);
                    break;
                case JArray a:
                    AddToArray(a);
                    break;
            }
        }

        private void AddProperty(string propertyName, JObject o)
        {
            o.Add(propertyName, GetValue(propertyName, o));
            options.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"Property {propertyName} added to object: {o.Path}");
        }

        private void AddToArray(JArray jArray)
        {
            if (functionAnalysis.IsFunction)
                jArray.Add(ExecuteFunction(jArray, options, functionAnalysis));
            else
                jArray.Add(Value);
            options.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"Value added to array: {jArray.Path}");
        }

        private JToken ExecuteFunction(JToken data, IJLioExecutionOptions jLioExecutionOptions,
            FunctionAnalysis functionAnalysis)
        {
            return JToken.Parse("Support for functions need to be implemented");
        }

        private JToken GetValue(string propertyName, JObject o)
        {
            if (functionAnalysis.IsFunction)
                return ExecuteFunction(o[propertyName], options, functionAnalysis);

            return Value;
        }
    }
}