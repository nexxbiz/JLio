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
    public class Copy : IJLioCommand
    {
        public string CommandName => "copy";
        private IJLioExecutionOptions executionOptions;

        public Copy()
        {

        }

        public Copy(string from, string to)
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
            return CopyMove.Copy(FromPath, ToPath).Execute(dataContext,options);
        }

        public bool ValidateCommandInstance()
        {
            var result = true;
            if (string.IsNullOrWhiteSpace(FromPath))
            {
                executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                    $"Path property for {CommandName} command is missing");
                result = false;
            }

            if (string.IsNullOrWhiteSpace(ToPath))
            {
                executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution,
                    $"Path property for {CommandName} command is missing");
                result = false;
            }

            if (result == false)
                executionOptions.Logger?.Log(LogLevel.Warning, JLioConstants.CommandExecution, "Command not executed");
            return result;
        }
    }
}
