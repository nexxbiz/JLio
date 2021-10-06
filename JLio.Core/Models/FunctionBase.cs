using System.Collections.Generic;
using System.Linq;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public abstract class FunctionBase : IFunction
    {
        public Arguments arguments = new Arguments();

        public string FunctionName => GetType().Name.CamelCasing();

        public IFunction SetArguments(Arguments functionArguments)
        {
            arguments = functionArguments;
            return this;
        }

        public string ToScript()
        {
            return
                $"{FunctionName}({string.Join(CoreConstants.ArgumentsDelimiter.ToString(), arguments.Select(a => a.Function.ToScript()))})";
        }

        public abstract JLioFunctionResult Execute(JToken currentToken, JToken dataContext, IExecutionContext context);

        public static List<JToken> GetArguments(Arguments arguments, JToken currentToken, JToken dataContext,
            IExecutionContext context)
        {
            var argumentValues = new List<JToken>();
            arguments.ForEach(a => argumentValues.Add(
                a.GetValue(currentToken, dataContext, context).Data.GetJTokenValue()));
            return argumentValues;
        }
    }
}