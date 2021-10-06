using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    //> - concat(arguments[])

    public class Concat : FunctionBase
    {
        public Concat()
        {
        }

        public Concat(params string[] arguments)
        {
            arguments.ToList().ForEach(a =>
                this.arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{a}\"")))));
        }

        public override JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context)
        {
            var argumentValues = GetArguments(arguments, currentToken, dataContext, context)
                .Select(i => i.ToString().Trim(CoreConstants.StringIndicator));
            var concatenatedString = string.Concat(argumentValues);
            return new JLioFunctionResult(true, new JValue(concatenatedString));
        }
    }
}