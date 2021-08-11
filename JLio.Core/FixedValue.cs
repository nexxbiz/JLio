using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class FixedValue : IJLioFunction
    {
        private readonly JToken value;
        private Arguments arguments;

        public FixedValue(JToken value)
        {
            this.value = value;
        }

        public string FunctionName => "FixedValue";

        public IJLioFunction SetArguments(Arguments newArguments)
        {
            arguments = newArguments;
            return this;
        }

        public JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IJLioExecutionOptions options)
        {
            return new JLioExecutionResult(true, value);
        }

        public string ToScriptString()
        {
            return value.ToString();
        }
    }
}