using System.Collections.Generic;
using JLio.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class JLioScript : List<IJLioCommand>
    {
     

        public JLioExecutionResult Execute(JToken data)
        {
            return Execute(data, JLioExecutionOptions.CreateDefault());
        }

        public JLioExecutionResult Execute(JToken data, IJLioExecutionOptions options)
        {
            JLioExecutionResult executionResult = new JLioExecutionResult(true,data);
            ForEach(command =>
                executionResult = command.Execute(data, options)
            );
            return executionResult;
        }

        public void AddLine(IJLioCommand command)
        {
            base.Add(command);
        }
    }
}