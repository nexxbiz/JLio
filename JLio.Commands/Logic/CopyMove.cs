using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JLio.Commands.Logic
{
    // source property -> copy move to property (value will be copy moved to the destination)
    // source property -> copy move to Object: the object will get the value of the source
    // source property -> copy move to array (add property value to array)
    // source array / multiple items -> copy move to property (property will become an array - any initial value will be lost)
    // source array / multiple items -> copy move to object (object will become an array - any initial value will be lost)
    // source array / multiple items -> copy move to array / multiple items (the whole array will be added to the array)
    // source array[*] / multiple items -> copy move to array / multiple items (the items will be added to the array)

    // in all cases it the target doesn't exists the target will be created

    internal enum eAction
    {
        Copy,
        Move
    }

    internal class CopyMove
    {
        private eAction action;
        private JToken data;
        private IJLioExecutionOptions executionOptions;

        private string from;

        private string to;

        internal static CopyMove Move(string from, string to)
        {
            return new CopyMove { from = from, to = to, action = eAction.Move };
        }

        internal static CopyMove Copy(string from, string to)
        {
            return new CopyMove { from = from, to = to, action = eAction.Copy };
        }

        internal JLioExecutionResult Execute(JToken dataContext, IJLioExecutionOptions options)
        {
            data = dataContext;
            executionOptions = options;


            var sourceItems = options.ItemsFetcher.SelectTokens(from, data);
            sourceItems.ForEach(i =>
            {
                AddToTargets(i);
                if (action == eAction.Move)
                    RemoveItemFromSource(i);
            });

            return new JLioExecutionResult(true,dataContext);
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
                    executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                        "remove only works on properties or items in array's");
                    break;
            }
        }

        private void RemoveValuesFromArray(JArray array, JToken selectedValue)
        {
            var index = array.IndexOf(selectedValue);
            array.RemoveAt(index);
        }

        private void AddToTargets(JToken value)
        {
            var toPath = JsonPathMethods.SplitPath(to);
            CreateParentTargets(toPath);
            var targetItems = executionOptions.ItemsFetcher.SelectTokens(toPath.ParentElements.ToPathString(), data);
            targetItems.ForEach(t => AddToTarget(toPath.LastName, t, value));
        }

        private SelectedTokens CreateParentTargets(JsonSplittedPath toPath)
        {
            JsonMethods.CheckOrCreateParentPath(data, toPath, executionOptions.ItemsFetcher, executionOptions.Logger);
            return executionOptions.ItemsFetcher.SelectTokens(to, data);
        }

        private void AddToTarget(string propertyName, JToken jToken, JToken value)
        {
            switch (jToken)
            {
                case JObject o:
                    if (JsonMethods.IsPropertyOfTypeArray(propertyName, o))
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
            executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"Property {propertyName} added to object: {o.Path}");
        }

        private void AddToArray(JArray jArray, JToken value)
        {
            jArray.Add(value);
            executionOptions.Logger?.Log(LogLevel.Information, JLioConstants.CommandExecution,
                $"Value added to array: {jArray.Path}");
        }
    }
}
