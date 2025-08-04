using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace JLio.Core;

public class FixedValue : IFunction
{
    public static FunctionConverter DefaultFunctionConverter { get; set; } =
        new FunctionConverter(new FunctionsProvider());

    public FixedValue(JToken value, FunctionConverter functionConverter = null)
    {
        Value = value;
        FunctionConverter = functionConverter ?? DefaultFunctionConverter;
    }

    public JToken Value { get; }

    public string FunctionName => "FixedValue";

    public FunctionConverter FunctionConverter { get; set; }

    public IFunction SetArguments(Arguments newArguments)
    {
        return this;
    }

    public JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (Value.Type == JTokenType.String) return HandleString(currentToken, dataContext, context);
        if (FunctionConverter != null && (Value.Type == JTokenType.Object || Value.Type == JTokenType.Array)) return ProcessToken(Value, currentToken, dataContext, context);
        return new JLioFunctionResult(true, Value);
    }

    private JLioFunctionResult ProcessToken(JToken token, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var clone = token.DeepClone();
        if (token.Type == JTokenType.Object) ProcessObject((JObject)clone, currentToken, dataContext, context);
        if (token.Type == JTokenType.Array) ProcessArray((JArray)clone, currentToken, dataContext, context);
        return new JLioFunctionResult(true, clone);
    }

    private void ProcessObject(JObject obj, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        foreach (var prop in obj.Properties().ToList())
        {
            prop.Value = ProcessValue(prop.Value, currentToken, dataContext, context);
        }
    }

    private void ProcessArray(JArray array, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        for (int i = 0; i < array.Count; i++)
            array[i] = ProcessValue(array[i], currentToken, dataContext, context);
    }

    private JToken ProcessValue(JToken value, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (IsFunction(value)) return ExecuteFunction(value.ToString(), currentToken, dataContext, context);

        if (value.Type == JTokenType.Object || value.Type == JTokenType.Array)
        {
            // FIX: Use the return value from ProcessToken instead of discarding it
            var result = ProcessToken(value, currentToken, dataContext, context);
            return result.Success ? result.Data.FirstOrDefault() ?? value : value;
        }

        return value;
    }

    private bool IsFunction(JToken token)
    {
        return token.Type == JTokenType.String && token.ToString().StartsWith("=");
    }

    private JToken ExecuteFunction(string expression, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        if (FunctionConverter == null)
        {
            context.LogError(CoreConstants.FunctionExecution, "FunctionsConverter is not set for FixedValue execution.");
            return JValue.CreateString(expression); // or throw an exception
        }
        var function = FunctionConverter.ParseString(expression);
        var result = function.GetValue(currentToken, dataContext, context); // Execute the function with the current context
        if (!result.Success)
            return JValue.CreateString(expression);
        return result.Data.FirstOrDefault(); // Return the result of the function execution   

    }

    public string ToScript()
    {
        return Value.ToString();
    }

    private JLioFunctionResult HandleString(JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var stringValue = Value.ToString();

        // Handle parent item navigation
        if (stringValue.StartsWith(context.ItemsFetcher.CurrentItemPathIndicator, StringComparison.InvariantCulture))
        {
            var pathAfterCurrent = stringValue.Substring(context.ItemsFetcher.CurrentItemPathIndicator.Length);
            
            // Check if the path contains parent indicators
            if (pathAfterCurrent.StartsWith(context.ItemsFetcher.PathDelimiter + context.ItemsFetcher.ParentPathIndicator, StringComparison.InvariantCulture))
            {
                return HandleParentNavigation(stringValue, currentToken, dataContext, context);
            }
        }

        if (stringValue.StartsWith(context.ItemsFetcher.CurrentItemPathIndicator,
            StringComparison.InvariantCulture))
            return new JLioFunctionResult(true,
                context.ItemsFetcher.SelectTokens(
                    $"{context.ItemsFetcher.RootPathIndicator}{stringValue.Substring(context.ItemsFetcher.CurrentItemPathIndicator.Length)}"
                    , currentToken));
        if (stringValue.StartsWith(context.ItemsFetcher.RootPathIndicator, StringComparison.InvariantCulture))
            return new JLioFunctionResult(true, context.ItemsFetcher.SelectTokens(stringValue, dataContext));
        return new JLioFunctionResult(true, Value);
    }

    private JLioFunctionResult HandleParentNavigation(string stringValue, JToken currentToken, JToken dataContext, IExecutionContext context)
    {
        var pathAfterCurrent = stringValue.Substring(context.ItemsFetcher.CurrentItemPathIndicator.Length);
        
        // Remove the leading delimiter if present
        if (pathAfterCurrent.StartsWith(context.ItemsFetcher.PathDelimiter))
        {
            pathAfterCurrent = pathAfterCurrent.Substring(context.ItemsFetcher.PathDelimiter.Length);
        }

        var parentIndicator = context.ItemsFetcher.ParentPathIndicator;
        var parentIndicatorWithDelimiter = parentIndicator + context.ItemsFetcher.PathDelimiter;
        
        // Count parent levels and extract remainder path
        var parentLevels = 0;
        var remainderPath = pathAfterCurrent;
        
        while (remainderPath.StartsWith(parentIndicatorWithDelimiter, StringComparison.InvariantCulture))
        {
            parentLevels++;
            remainderPath = remainderPath.Substring(parentIndicatorWithDelimiter.Length);
        }
        
        // Check if the path ends with a parent indicator (no delimiter after)
        if (remainderPath == parentIndicator)
        {
            parentLevels++;
            remainderPath = "";
        }

        // Navigate up the parent hierarchy
        var targetToken = currentToken;
        for (int i = 0; i < parentLevels && targetToken != null; i++)
        {
            var nextParent = NavigateToSemanticParent(targetToken);
            if (nextParent == null)
            {
                // Can't navigate up enough levels, return the original value
                return new JLioFunctionResult(true, Value);
            }
            targetToken = nextParent;
        }

        // If we couldn't navigate up enough levels, return the original value
        if (targetToken == null)
        {
            return new JLioFunctionResult(true, Value);
        }

        // If there's a remainder path, apply it to the target token
        if (!string.IsNullOrEmpty(remainderPath))
        {
            var finalPath = $"{context.ItemsFetcher.CurrentItemPathIndicator}{context.ItemsFetcher.PathDelimiter}{remainderPath}";
            return new JLioFunctionResult(true,
                context.ItemsFetcher.SelectTokens(
                    $"{context.ItemsFetcher.RootPathIndicator}{context.ItemsFetcher.PathDelimiter}{remainderPath}",
                    targetToken));
        }
        else
        {
            // No remainder path, return the target token itself
            return new JLioFunctionResult(true, new SelectedTokens(targetToken));
        }
    }

    private JToken NavigateToSemanticParent(JToken token)
    {
        if (token?.Parent == null)
            return null;

        var parent = token.Parent;
        
        // If we're an array element, the parent is the array
        // If the array is a property value, we need to go to the object containing the property
        if (token.Parent is JArray)
        {
            // Parent is an array, check if this array is a property value
            if (parent.Parent is JProperty)
            {
                // Skip the JProperty and go to the containing object
                return parent.Parent.Parent;
            }
            // Array is not a property value (root array), return the array itself
            return parent;
        }
        
        // If we're a property value (object/primitive), the semantic parent is the object containing the property
        if (token.Parent is JProperty property)
        {
            return property.Parent;
        }
        
        // For other cases, just return the direct parent
        return parent;
    }
}