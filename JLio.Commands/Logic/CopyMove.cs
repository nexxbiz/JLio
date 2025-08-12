using System;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Logic;

// source property -> copy move to property (value will be copy moved to the destination)
// source property -> copy move to Object: the object will get the value of the source
// source property -> copy move to array (add property value to array)
// source array / multiple items -> copy move to property (property will become an array - any initial value will be lost)
// source array / multiple items -> copy move to object (object will become an array - any initial value will be lost)
// source array / multiple items -> copy move to array / multiple items (the whole array will be added to the array)
// source array[*] / multiple items -> copy move to array / multiple items (the items will be added to the array)

// in all cases it the target doesn't exists the target will be created

public abstract class CopyMove : CommandBase
{
    [JsonProperty("destinationAsArray")]
    public bool DestinationAsArray { get; set; } = false;
    //private eAction action;
    private JToken data;
    private IExecutionContext executionContext;

    [JsonProperty("fromPath")]
    public string FromPath { get; set; }

    [JsonProperty("toPath")]
    public string ToPath { get; set; }

    internal JLioExecutionResult Execute(JToken dataContext, IExecutionContext context, EAction action)
    {
        data = dataContext;
        executionContext = context;
        var sourceItems = context.ItemsFetcher.SelectTokens(FromPath, data);
        var OrgPath = ToPath;
        
        // 🎯 FIXED: Resolve function expressions in ToPath (including indirect)  
        if (ToPath.StartsWith("="))
        {
            try
            {
                // Use the ItemsFetcher's indirect processing logic directly
                // This mimics what ProcessIndirectPath does in JsonPathItemsFetcher
                if (ToPath.Contains("=indirect("))
                {
                    var indirectPattern = new System.Text.RegularExpressions.Regex(@"=indirect\(([^)]+)\)", 
                        System.Text.RegularExpressions.RegexOptions.Compiled | System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                    var match = indirectPattern.Match(ToPath);
                    if (match.Success)
                    {
                        var indirectPathReference = match.Groups[1].Value.Trim();
                        
                        // Remove quotes if present
                        if ((indirectPathReference.StartsWith("'") && indirectPathReference.EndsWith("'")) ||
                            (indirectPathReference.StartsWith("\"") && indirectPathReference.EndsWith("\"")))
                        {
                            indirectPathReference = indirectPathReference.Substring(1, indirectPathReference.Length - 2);
                        }

                        // Get the value at the indirect path reference
                        var indirectPathToken = dataContext.SelectToken(indirectPathReference);
                        if (indirectPathToken != null && indirectPathToken.Type == JTokenType.String)
                        {
                            var actualPath = indirectPathToken.Value<string>();
                            if (!string.IsNullOrEmpty(actualPath))
                            {
                                // Replace the =indirect(...) part with the actual path
                                ToPath = indirectPattern.Replace(ToPath, actualPath);
                            }
                        }
                    }
                }
                else
                {
                    // Fallback: try the old method with function converter for other functions
                    var functionConverter = FixedValue.DefaultFunctionConverter;
                    var function = functionConverter.ParseString(ToPath);
                    var result = function.GetValue(dataContext, dataContext, context);
                    
                    if (result.Success && result.Data.Any())
                    {
                        var resolvedPath = result.Data.FirstOrDefault()?.ToString();
                        if (!string.IsNullOrEmpty(resolvedPath))
                        {
                            ToPath = resolvedPath;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                context.LogWarning(CoreConstants.CommandExecution, 
                    $"Failed to evaluate function expression in toPath: {ToPath}. Error: {ex.Message}");
            }
        }
        
        // Check if ToPath is a root path reference
        if (ToPath == executionContext.ItemsFetcher.RootPathIndicator)
            return HandleRootObject(dataContext, sourceItems);
            
        var innerArrayIndex = GetInnerArrayIndex();
        if (innerArrayIndex != -1)
            HandleActionPerSource(action, sourceItems, innerArrayIndex);
        else
            HandleActionForEachToAll(action, sourceItems);

        ToPath = OrgPath;
        return new JLioExecutionResult(true, dataContext);
    }

    private int GetInnerArrayIndex()
    {
        var fromPath = JsonPathMethods.SplitPath(FromPath);
        var toPath = JsonPathMethods.SplitPath(ToPath);
        if (!fromPath.HasArrayIndication || !toPath.HasArrayIndication) return -1;
        var index = fromPath.GetSameElementsIndex(toPath);
        if (!HasArrayNotationAfterIndex(fromPath, index) || index > 1) 
            return index;

        return -1;
    }

    private static bool HasArrayNotationAfterIndex(JsonSplittedPath path, int index)
    {
        return path.Elements.Skip(index).Any(e => e.HasArrayIndicator);
    }

    private void HandleActionPerSource(EAction action, SelectedTokens sourceItems, int innerArrayIndex)
    {
        var targetPath = JsonPathMethods.SplitPath(ToPath).Elements.Skip(innerArrayIndex);
        sourceItems.ForEach(i =>
        {
            var selectionTargetPath =
                new JsonSplittedPath(JsonPathMethods.SplitPath(i.Path).Elements.Take(innerArrayIndex - 1));
            selectionTargetPath.Elements.AddRange(targetPath);
            AddToTargets(i, selectionTargetPath);
            if (action == EAction.Move)
                RemoveItemFromSource(i);
        });
    }

    private void HandleActionForEachToAll(EAction action, SelectedTokens sourceItems)
    {
        var toPath = JsonPathMethods.SplitPath(ToPath);
        sourceItems.ForEach(i =>
        {
            AddToTargets(i, toPath);
            if (action == EAction.Move)
                RemoveItemFromSource(i);
        });
    }

    private JLioExecutionResult HandleRootObject(JToken dataContext, SelectedTokens sourceItems)
    {
        dataContext.Children().ToList().ForEach(c => c.Remove());
        if (dataContext is JObject o) o.Merge(sourceItems.First());
        else
            executionContext.LogWarning(CoreConstants.CommandExecution,
                "copy/move to root object will only work for an source type: object");

        return new JLioExecutionResult(true, dataContext);
    }

    private void RemoveItemFromSource(JToken selectedValue)
    {
        var parent = selectedValue.Parent;
        switch (parent)
        {
            case JProperty p:
                p.Remove();
                break;
            case JArray a:
                RemoveValuesFromArray(a, selectedValue);
                break;
            default:
                executionContext.LogWarning(CoreConstants.CommandExecution,
                    "remove only works on properties or items in array's");
                break;
        }
    }

    private void RemoveValuesFromArray(JArray array, JToken selectedValue)
    {
        var index = array.IndexOf(selectedValue);
        array.RemoveAt(index);
    }

    private void AddToTargets(JToken value, JsonSplittedPath toPath)
    {
        JsonMethods.CheckOrCreateParentPath(data, toPath, executionContext.ItemsFetcher,
            executionContext.Logger);
        var targetItems = executionContext.ItemsFetcher.SelectTokens(toPath.ParentElements.ToPathString(), data);
        targetItems.ForEach(t => AddToTarget(toPath.LastName, t, value));
    }

    private void AddToTarget(string propertyName, JToken jToken, JToken value)
    {
        switch (jToken)
        {
            case JObject o:
                if (DestinationAsArray)
                {
                    // If property exists and is not array, convert to array with old value
                    if (o.ContainsKey(propertyName))
                    {
                        if (o[propertyName] is JArray arr)
                        {
                            AddToArray(arr, value);
                        }
                        else
                        {
                            var oldVal = o[propertyName];
                            var newArr = new JArray();
                            if (oldVal != null && oldVal.Type != JTokenType.Null)
                                newArr.Add(oldVal);
                            newArr.Add(value);
                            o[propertyName] = newArr;
                            executionContext.LogInfo(CoreConstants.CommandExecution, $"Property {propertyName} converted to array and value added: {o.Path}");
                        }
                    }
                    else
                    {
                        // Property does not exist, create as array
                        var newArr = new JArray(value);
                        o[propertyName] = newArr;
                        executionContext.LogInfo(CoreConstants.CommandExecution, $"Property {propertyName} created as array: {o.Path}");
                    }
                    return;
                }
                else if (JsonMethods.IsPropertyOfTypeArray(propertyName, o))
                {
                    AddToArray((JArray)o[propertyName], value);
                    return;
                }
                else if (o.ContainsKey(propertyName))
                {
                    o[propertyName] = value;
                    return;
                }
                AddProperty(propertyName, o, value);
                break;
            case JArray a:
                AddToArray(a, value);
                break;
        }
    }

    private void AddProperty(string propertyName, JObject o, JToken value)
    {
        o.Add(propertyName, value);
        executionContext.LogInfo(CoreConstants.CommandExecution,
            $"Property {propertyName} added to object: {o.Path}");
    }

    private void AddToArray(JArray jArray, JToken value)
    {
        jArray.Add(value);
        executionContext.LogInfo(CoreConstants.CommandExecution,
            $"Value added to array: {jArray.Path}");
    }
}