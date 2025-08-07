using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands;

public class Add : PropertyChangeCommand
{
    public Add()
    {
    }

    public Add(string path, JToken value)
    {
        Path = path;
        Value = new FunctionSupportedValue(new FixedValue(value));
    }

    public Add(string path, IFunctionSupportedValue value)
    {
        Path = path;
        Value = value;
    }

    // New constructor for new syntax
    public Add(string path, string property, JToken value)
    {
        Path = path;
        Property = property;
        Value = new FunctionSupportedValue(new FixedValue(value));
    }

    // New constructor for new syntax with function support
    public Add(string path, string property, IFunctionSupportedValue value)
    {
        Path = path;
        Property = property;
        Value = value;
    }

    internal override void ApplyValueToTarget(string propertyName, JToken jToken, JToken dataContext)
    {
        switch (jToken)
        {
            case JObject o:
                // Handle case where propertyName is null (shouldn't happen for objects)
                if (string.IsNullOrEmpty(propertyName))
                {
                    executionContext.LogWarning(CoreConstants.CommandExecution,
                        $"Property name is required for adding to objects. {CommandName} function not applied to {o.Path}");
                    return;
                }

                if (JsonMethods.IsPropertyOfTypeArray(propertyName, o))
                {
                    AddToArray((JArray)o[propertyName], dataContext);
                    return;
                }
                else if (o.ContainsKey(propertyName))
                {
                    executionContext.LogWarning(CoreConstants.CommandExecution,
                        $"Property {propertyName} already exists on {o.Path}. {CommandName} function not applied");
                    return;
                }

                AddProperty(propertyName, o, dataContext);
                break;
            case JArray a:
                // For arrays, propertyName can be null (direct array operation)
                AddToArray(a, dataContext);
                break;
        }
    }

}