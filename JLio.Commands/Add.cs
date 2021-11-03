using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands
{
    public class Add : PropertyChangeLogic
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

        internal override void ApplyValueToTarget(string propertyName, JToken jToken, JToken dataContext)
        {
            switch (jToken)
            {
                case JObject o:
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
                  
                        AddToArray(a, dataContext);
                    break;
            }
        }

    }
}