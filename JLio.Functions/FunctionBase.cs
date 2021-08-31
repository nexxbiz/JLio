using System.Collections.Generic;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public abstract class FunctionBase : IFunction
    {
        public Arguments arguments = new Arguments();

        public FunctionBase(string functionName)
        {
            FunctionName = functionName;
        }

        public string FunctionName { get; }

        public IFunction SetArguments(Arguments functionArguments)
        {
            arguments = functionArguments;
            return this;
        }

        public string ToScript()
        {
            return
                $"{FunctionName}({string.Join(JLioConstants.ArgumentsDelimiter.ToString(), arguments.Select(a => a.Function.ToScript()))})";
        }

        public abstract JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options);

        public static List<string> GetArgumentStrings(Arguments arguments, JToken currentToken, JToken dataContext,
            IExecutionOptions options)
        {
            var argumentValues = new List<string>();
            arguments.ForEach(a => argumentValues.Add(a.GetValue(currentToken, dataContext, options).ToString()));
            return argumentValues;
        }
    }
}