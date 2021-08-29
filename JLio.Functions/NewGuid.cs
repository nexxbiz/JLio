using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace JLio.Functions
{
    public class NewGuid : IFunction
    {
        public string FunctionName => "newGuid";

        public JLioExecutionResult Execute(JToken currentToken, JToken dataContext, IExecutionOptions options)
        {
            return new JLioExecutionResult(true,new JValue(Guid.NewGuid().ToString()));
        }

        public IFunction SetArguments(Arguments arguments)
        {
            return this;  
        }

        public string ToScript()
        {
            return $"{FunctionName}()";
        }
    }
}
