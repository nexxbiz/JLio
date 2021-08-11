using System.Collections.Generic;
using JLio.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    //needs to get a way to alter the executor. (inject? options?) 
    // now a fixed execution is implemented
    // there needs to be a way to execute the same script in 2 different way like production and debug
    // this also means that the signature result of the execute method could change
    // perhaps 2 different results can be in place result and debugResult

    public class CommandLines : List<IJLioCommand>
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
    }
}