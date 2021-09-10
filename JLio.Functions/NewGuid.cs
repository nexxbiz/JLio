using System;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Functions
{
    public class NewGuid : FunctionBase
    {
        public NewGuid() : base("newGuid")
        {
        }

        public override JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            return new JLioExecutionResult(true, new JValue(Guid.NewGuid().ToString()));
        }
    }
}