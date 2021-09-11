using JLio.Core.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Models
{
    public abstract class CommandBase : IJLioCommand
    {
        [JsonProperty("command")]
        public string CommandName => FirstCharToLowerCase(GetType().Name);

        public abstract JLioExecutionResult Execute(JToken dataContext, IExecutionOptions options);

        public abstract ValidationResult ValidateCommandInstance();

        private string FirstCharToLowerCase(string str)
        {
            if (string.IsNullOrEmpty(str) || char.IsLower(str[0]))
                return str;

            return char.ToLower(str[0]) + str.Substring(1);
        }
    }
}