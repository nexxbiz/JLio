using JLio.Core.Contracts;
using JLio.Core.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public abstract class CommandBase : ICommand
    {
        [JsonProperty("command")]
        public string CommandName => GetType().Name.CamelCasing();

        public abstract JLioExecutionResult Execute(JToken dataContext, IExecutionOptions options);

        public abstract ValidationResult ValidateCommandInstance();
    }
}