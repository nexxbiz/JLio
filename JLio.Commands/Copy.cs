using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace JLio.Commands
{
    public class Copy : CopyMove, IJLioCommand
    {
        private IExecutionOptions executionOptions;

        public Copy()
        {
        }

        public Copy(string from, string to)
        {
            FromPath = from;
            ToPath = to;
        }

        public string CommandName => "copy";

        public JLioExecutionResult Execute(JToken dataContext, IExecutionOptions options)
        {
            executionOptions = options;
            var validationResult = ValidateCommandInstance();
            if (!validationResult.IsValid)
            {
                validationResult.ValidationMessages.ForEach(i =>
                    options.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution, i));
                return new JLioExecutionResult(false, dataContext);
            }

            ;
            return Execute(dataContext, options, eAction.Copy);
        }

        public ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult {IsValid = true};
            if (string.IsNullOrWhiteSpace(FromPath))
            {
                result.ValidationMessages.Add($"FromPath property for {CommandName} command is missing");
                result.IsValid = false;
            }

            if (string.IsNullOrWhiteSpace(ToPath))
            {
                result.ValidationMessages.Add($"ToPath property for {CommandName} command is missing");
                result.IsValid = false;
            }

            return result;
        }
    }
}