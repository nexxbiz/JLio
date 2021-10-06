using JLio.Core.Contracts;
using JLio.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public abstract class CommandBase : ICommand
    {
        private bool executionFailed;

        [JsonProperty("command")]
        public string CommandName => GetType().Name.CamelCasing();

        public abstract JLioExecutionResult Execute(JToken dataContext, IExecutionContext context);

        public abstract ValidationResult ValidateCommandInstance();

        public void SetExecutionResult(JLioFunctionResult functionResult)
        {
            if (!functionResult.Success) executionFailed = true;
        }

        public void ResetExecutionSucces()
        {
            executionFailed = false;
        }

        public bool GetExecutionSucces()
        {
            return !executionFailed;
        }
    }
}