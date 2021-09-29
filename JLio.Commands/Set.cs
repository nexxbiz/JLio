﻿using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands
{
    public class Set : CommandBase
    {
        private IExecutionContext executionContext;

        public Set()
        {
        }

        public Set(string path, JToken value)
        {
            Path = path;
            Value = new FunctionSupportedValue(new FixedValue(value));
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

        public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
        {
            executionContext = context;
            ResetExecutionSucces();
            var validationResult = ValidateCommandInstance();
            if (!validationResult.IsValid)
            {
                validationResult.ValidationMessages.ForEach(i =>
                    context.LogWarning(CoreConstants.CommandExecution, i));
                return new JLioExecutionResult(false, dataContext);
            }

            //var targetPath = JsonPathMethods.SplitPath(Path);
            var targets = executionContext.ItemsFetcher.SelectTokens(Path, dataContext);

            targets.ForEach(i =>
            {
                SetValueToObjectItems(dataContext, new JsonSplittedPath(i.Path));
                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"{CommandName}: completed for {i.Path}");
            });

            return new JLioExecutionResult(GetExecutionSucces(), dataContext);
        }

        public override ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult();
            if (string.IsNullOrWhiteSpace(Path))
                result.ValidationMessages.Add($"Path property for {CommandName} command is missing");

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
                executionContext.ItemsFetcher.SelectTokens(path, dataContext);

            targetItems.ForEach(i => SetValueToTarget(targetPath.LastName, i, dataContext));
        }

        private void SetValueToTarget(string propertyName, JToken jToken, JToken dataContext)
        {
            switch (jToken)
            {
                case JObject o:
                    if (!o.ContainsKey(propertyName) &&
                        executionContext.ItemsFetcher.SelectToken(propertyName, o) != null)
                        ReplaceTargetTokenWithNewValue(o.SelectToken(propertyName), dataContext);

                    if (!o.ContainsKey(propertyName))
                    {
                        executionContext.LogInfo(CoreConstants.CommandExecution,
                            $"Property {propertyName} does not exists on {o.Path}. {CommandName} function not applied.");
                        return;
                    }

                    ReplaceCurrentValueWithNew(propertyName, o, dataContext);
                    break;
                case JArray a:
                    executionContext.LogInfo(CoreConstants.CommandExecution,
                        $"can't set value on a array on {a.Path}. {CommandName} functionality not applied.");
                    break;
            }
        }

        private void ReplaceTargetTokenWithNewValue(JToken currentJObject, JToken dataContext)
        {
            var valueResult = Value.GetValue(currentJObject, dataContext, executionContext);
            SetExecutionResult(valueResult);
            currentJObject.Replace(valueResult.Data.GetJTokenValue());
            executionContext.LogInfo(CoreConstants.CommandExecution,
                $"Value has been set on object at path {currentJObject.Path}.");
        }

        private void ReplaceCurrentValueWithNew(string propertyName, JObject o, JToken dataContext)
        {
            var valueResult = Value.GetValue(o[propertyName], dataContext, executionContext);
            SetExecutionResult(valueResult);
            o[propertyName] = valueResult.Data.GetJTokenValue();

            executionContext.LogInfo(CoreConstants.CommandExecution,
                $"Property {propertyName} on {o.Path} value has been set.");
        }
    }
}