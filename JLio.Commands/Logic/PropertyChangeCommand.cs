using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Logic
{
    public abstract class PropertyChangeCommand : CommandBase
    {
        internal IExecutionContext executionContext;

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

            var targetPath = JsonPathMethods.SplitPath(Path);
            JsonMethods.CheckOrCreateParentPath(dataContext, targetPath, context.ItemsFetcher, context.Logger);
            AddToObjectItems(dataContext, context.ItemsFetcher, targetPath);
            context.LogInfo(CoreConstants.CommandExecution,
                $"{CommandName}: completed for {targetPath.Elements.ToPathString()}");
            return new JLioExecutionResult(GetExecutionSucces(), dataContext);
        }

        public override ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult();
            if (string.IsNullOrWhiteSpace(Path))
                result.ValidationMessages.Add($"Path property for {CommandName} command is missing");

            return result;
        }

        protected void AddToObjectItems(JToken dataContext, IItemsFetcher dataFetcher, JsonSplittedPath targetPath)
        {
            var path = targetPath.Elements
                .Take(targetPath.Elements.Count() - 1)
                .ToPathString();
            if (targetPath.IsSearchingForObjectsByName)
                path = targetPath.Elements.ToPathString();
            var targetItems =
                dataFetcher.SelectTokens(path, dataContext);
            targetItems.ForEach(i => ApplyValueToTarget(targetPath.LastName, i, dataContext));
        }

        internal abstract void ApplyValueToTarget(string propertyName, JToken jToken, JToken dataContext);

        //internal void SetProperty(string propertyName, JObject o, JToken dataContext)
        //{
        //    var valueResult = Value.GetValue(o, dataContext, executionContext);
        //    SetExecutionResult(valueResult);
        //    o[propertyName] = valueResult.Data.GetJTokenValue();
        //    executionContext.LogInfo(CoreConstants.CommandExecution,
        //        $"Property {propertyName} set to object: {o.Path}");
        //}

        internal void AddProperty(string propertyName, JObject o, JToken dataContext)
        {
            var valueResult = Value.GetValue(o, dataContext, executionContext);
            SetExecutionResult(valueResult);
            o.Add(propertyName, valueResult.Data.GetJTokenValue());
            executionContext.LogInfo(CoreConstants.CommandExecution,
                $"Property {propertyName} added to object: {o.Path}");
        }

        internal void AddToArray(JArray jArray, JToken dataContext)
        {
            var valueResult = Value.GetValue(jArray, dataContext, executionContext);
            SetExecutionResult(valueResult);
            jArray.Add(valueResult.Data);
            executionContext.LogInfo(CoreConstants.CommandExecution,
                $"Value added to array: {jArray.Path}");
        }

        internal void ReplaceTargetTokenWithNewValue(JToken currentJObject, JToken dataContext)
        {
            var valueResult = Value.GetValue(currentJObject, dataContext, executionContext);
            SetExecutionResult(valueResult);
            currentJObject.Replace(valueResult.Data.GetJTokenValue());
            executionContext.LogInfo(CoreConstants.CommandExecution,
                $"Value has been set on object at path {currentJObject.Path}.");
        }

        internal void ReplaceCurrentValueWithNew(string propertyName, JObject o, JToken dataContext)
        {
            var valueResult = Value.GetValue(o[propertyName], dataContext, executionContext);
            SetExecutionResult(valueResult);
            o[propertyName] = valueResult.Data.GetJTokenValue();

            executionContext.LogInfo(CoreConstants.CommandExecution,
                $"Property {propertyName} on {o.Path} value has been set.");
        }
    }
}