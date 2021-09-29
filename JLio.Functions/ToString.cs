using System.Linq;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public class ToString : FunctionBase
    {
        public ToString()
        {
        }

        public ToString(string path)
        {
            arguments.Add(new FunctionSupportedValue(new FixedValue(JToken.Parse($"\"{path}\""))));
        }

        public override JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            if (arguments.Any()) return ExecuteWithArguments(currentToken, dataContext, options);

            return ExecuteWithoutArguments(currentToken);
        }

        private JLioExecutionResult ExecuteWithArguments(JToken currentToken, JToken dataContext,
            IExecutionOptions options)
        {
            var value = GetArgumentStrings(arguments, currentToken, dataContext, options);
            return new JLioExecutionResult(true, new JValue(value.FirstOrDefault()));
        }

        private JLioExecutionResult ExecuteWithoutArguments(JToken currentToken)
        {
            if (currentToken.Type == JTokenType.String) return new JLioExecutionResult(true, currentToken);

            return new JLioExecutionResult(true, new JValue(currentToken.ToString(Formatting.None)));
        }
    }
}