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
        public Concat() : base("concat")
        {
        }

        public Concat(params string[] arguments) : base("concat")
        {
            arguments.ToList().ForEach(a =>
                this.arguments.Add(new JLioFunctionSupportedValue(new FixedValue(JToken.Parse($"\"{a}\"")))));
        }

        public override JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            var argumentValues = GetArgumentStrings(arguments, currentToken, dataContext, options);
            var concatenatedString = string.Concat(argumentValues);
            return new JLioExecutionResult(true, new JValue(concatenatedString));
        }
    }
}