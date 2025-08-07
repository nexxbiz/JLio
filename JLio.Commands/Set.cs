using System.Linq;
using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands;

public class Set : PropertyChangeCommand
{
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

    // New constructor for new syntax
    public Set(string path, string property, JToken value)
    {
        Path = path;
        Property = property;
        Value = new FunctionSupportedValue(new FixedValue(value));
    }

    // New constructor for new syntax with function support
    public Set(string path, string property, IFunctionSupportedValue value)
    {
        Path = path;
        Property = property;
        Value = value;
    }

    public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
    {
        // Check if using new syntax, if so, use base class implementation
        if (!string.IsNullOrEmpty(Property))
        {
            return base.Execute(dataContext, context);
        }

        // Legacy implementation for backwards compatibility
        executionContext = context;
        ResetExecutionSucces();
        var validationResult = ValidateCommandInstance();
        if (!validationResult.IsValid)
        {
            validationResult.ValidationMessages.ForEach(i =>
                context.LogWarning(CoreConstants.CommandExecution, i));
            return new JLioExecutionResult(false, dataContext);
        }

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

        // Additional validation for new syntax
        if (!string.IsNullOrEmpty(Property))
        {
            // When using new syntax, validate that property name is reasonable
            if (Property.Contains(".") || Property.Contains("[") || Property.Contains("]"))
            {
                result.ValidationMessages.Add($"Property name '{Property}' should be a simple property name, not a path");
            }
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
            executionContext.ItemsFetcher.SelectTokens(path, dataContext);

        targetItems.ForEach(i => ApplyValueToTarget(targetPath.LastName, i, dataContext));
    }

    internal override void ApplyValueToTarget(string propertyName, JToken jToken, JToken dataContext)
    {
        switch (jToken)
        {
            case JObject o:
                // Handle case where propertyName is null (shouldn't happen for objects in Set)
                if (string.IsNullOrEmpty(propertyName))
                {
                    executionContext.LogWarning(CoreConstants.CommandExecution,
                        $"Property name is required for setting object properties. {CommandName} function not applied to {o.Path}");
                    return;
                }

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
                // Set doesn't typically work on arrays directly, but handle gracefully
                executionContext.LogInfo(CoreConstants.CommandExecution,
                    $"Set command cannot be applied directly to arrays. Use Add command instead for {a.Path}");
                break;
        }
    }
}