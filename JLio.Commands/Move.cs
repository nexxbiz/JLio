using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands
{
    public class Move : CopyMove
    {
        public Move()
        {
        }

        public Move(string from, string to)
        {
            FromPath = from;
            ToPath = to;
        }

        public override JLioExecutionResult Execute(JToken dataContext, IExecutionOptions options)
        {
            var validationResult = ValidateCommandInstance();
            if (!validationResult.IsValid)
            {
                validationResult.ValidationMessages.ForEach(i =>
                    options.LogWarning(CoreConstants.CommandExecution, i));
                return new JLioExecutionResult(false, dataContext);
            }

            return Execute(dataContext, options, EAction.Move);
        }

        public override ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult();
            if (string.IsNullOrWhiteSpace(FromPath))
                result.ValidationMessages.Add($"FromPath property for {CommandName} command is missing");

            if (string.IsNullOrWhiteSpace(ToPath))
                result.ValidationMessages.Add($"ToPath property for {CommandName} command is missing");

            return result;
        }
    }
}