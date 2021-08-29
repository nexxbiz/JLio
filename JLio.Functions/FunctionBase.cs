using System;
using System.Collections.Generic;
using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public abstract class FunctionBase :IFunction
    {
        public FunctionBase(string functionName)
        {
            FunctionName = functionName;
        }

        public Arguments arguments = new Arguments();
        public string FunctionName { get; }

        public static List<string> GetArgumentStrings(Arguments arguments, JToken currentToken, JToken dataContext,
            IExecutionOptions options)
        {
            var argumentValues = new List<string>();
            arguments.ForEach(a => argumentValues.Add(a.GetValue(currentToken, dataContext, options).ToString()));
            return argumentValues;
        }

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
    }
}