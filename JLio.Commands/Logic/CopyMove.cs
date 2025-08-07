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
                if (JsonMethods.IsPropertyOfTypeArray(propertyName, o))
                {
                    AddToArray((JArray) o[propertyName], value);
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