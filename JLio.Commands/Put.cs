using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands
{
    public class Put : PropertyChangeLogic
    {
        public Put()
        {
        }

        public Put(string path, JToken value)
        {
            Path = path;
            Value = new FunctionSupportedValue(new FixedValue(value));
        }

        public Put(string path, IFunctionSupportedValue value)
        {
            Path = path;
            Value = value;
        }

        internal override void ApplyValueToTarget(string propertyName, JToken jToken, JToken dataContext)
        {
            switch (jToken)
            {
                case JObject o:
                    if (JsonMethods.IsPropertyOfTypeArray(propertyName, o) || o.ContainsKey(propertyName))
                    {
                        SetProperty(propertyName, o, dataContext);
                        return;
                    }
                    AddProperty(propertyName, o, dataContext);
                    break;
                case JArray a:
                    SetProperty(propertyName, (JObject)a.Parent?.Parent, dataContext);
                    break;
            }
        }
    }
}