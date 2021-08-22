using System.Linq;
using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace JLio.Commands
{
    public class Move : IJLioCommand
    {
        public string CommandName => "move";
        private IJLioExecutionOptions executionOptions;

        public Move()
        {

        }

        public Move(string from, string to)
        {
            FromPath = from;
            ToPath = to;
        }

        [JsonProperty("fromPath")]
        public string FromPath { get; set; }

        [JsonProperty("toPath")]
        public string ToPath { get; set; }

        public JLioExecutionResult Execute(JToken dataContext, IJLioExecutionOptions options)
        {
            executionOptions = options;
            var validationResult = ValidateCommandInstance();
            if (!validationResult.IsValid)
            {
                validationResult.ValidationMessages.ForEach(i => options.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution, i));
                return new JLioExecutionResult(false, dataContext);
            };
            return CopyMove.Move(FromPath, ToPath).Execute(dataContext, options);
        }

        public ValidationResult ValidateCommandInstance()
        {
            var result = new ValidationResult() { IsValid = true };
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
