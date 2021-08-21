using System.Collections.Generic;
using JLio.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class JLioScript : List<IJLioCommand>
    {
     

        public JLioExecutionResults Execute(JToken data)
        {
            return Execute(data, JLioExecutionOptions.CreateDefault());
        }

        public JLioExecutionResults Execute(JToken data, IJLioExecutionOptions options)
        {
            var executionResult = new JLioExecutionResults();
            ForEach(command =>
                executionResult.Add(command.Execute(data, options))
            );
            return executionResult;
        }

        public void AddLine(IJLioCommand command)
        {
            base.Add(command);
        }
    }
}