using JLio.Core.Contracts;
using JLio.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace JLio.Core
{
    public class NotFoundCommand : IJLioCommand
    {
        protected internal NotFoundCommand(string fullCommandText)
        {
            CommandName = fullCommandText;
        }

        public JLioCommandRegistration CommandRegistration { get; set; }

        public string CommandName { get; }

        public JLioExecutionResult Execute(JToken data, IJLioExecutionOptions options)
        {
            options.Logger?.Log(LogLevel.Error, JLioConstants.CommandExecution,
                $"script contains a unknown command : {CommandName}");
            return new JLioExecutionResult(false, data);
        }

        public bool ValidateCommandInstance()
        {
            return true;
        }
    }
}