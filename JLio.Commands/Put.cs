using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands
{
    public class Put : AddPut
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
    }
}