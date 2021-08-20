using System.Collections.Generic;
using JLio.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class CommandLines 
    {
        List<IJLioCommand> Lines { get; set; }

        public CommandLines()
        {
            this.Lines = new List<IJLioCommand>();
        }

        public CommandLines(List<IJLioCommand> lines)
        {
            this.Lines = lines;
        }

        public JLioExecutionResults Execute(JToken data)
        {
            return Execute(data, JLioExecutionOptions.CreateDefault());
        }

        public JLioExecutionResults Execute(JToken data, IJLioExecutionOptions options)
        {
            var executionResult = new JLioExecutionResults();
            Lines.ForEach(command =>
                executionResult.Add(command.Execute(data, options))
            );
            return executionResult;
        }

        public void AddLine(IJLioCommand command)
        {
            Lines.Add(command);
        }
    }
}