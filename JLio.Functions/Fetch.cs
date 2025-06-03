using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public class Fetch : FunctionBase
    {
        public Fetch()
        {
        }

        public Fetch(string path)
        {
                Arguments.Add(new FunctionSupportedValue(new FixedValue(path)));
        }

        public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            

            var argumentValue = GetArguments(Arguments, currentToken, dataContext, context).FirstOrDefault();
                if(argumentValue == null)
            {
                return  new JLioFunctionResult(false, JValue.CreateNull());
            }
           
            return new JLioFunctionResult(true, argumentValue);
        }
    }
}