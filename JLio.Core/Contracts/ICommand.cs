using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Contracts
{
    public interface ICommand
    {
        string CommandName { get; }

        JLioExecutionResult Execute(JToken dataContext, IExecutionContext context);

        ValidationResult ValidateCommandInstance();
    }
}