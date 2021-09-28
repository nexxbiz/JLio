using System.Collections.Generic;
using System.Linq;
using JLio.Core.Contracts;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public class JLioScript : List<ICommand>
    {
        public JLioExecutionResult Execute(JToken data)
        {
            return Execute(data, ExecutionContext.CreateDefault());
        }

        public JLioExecutionResult Execute(JToken data, IExecutionContext context)
        {
            var executionResult = new JLioExecutionResult(true, data);
            ForEach(command =>
                executionResult = command.Execute(data, context)
            );
            return executionResult;
        }

        public void AddLine(ICommand command)
        {
            Add(command);
        }

        public bool Validate()
        {
            return this.All(l => l.ValidateCommandInstance().IsValid);
        }

        public List<ValidationResult> GetValidationResults()
        {
            var result = new List<ValidationResult>();
            ForEach(l => result.Add(l.ValidateCommandInstance()));
            return result;
        }
    }
}