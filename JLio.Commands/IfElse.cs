using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands;

public class IfElse : CommandBase
{
    [JsonProperty("first")]
    public IFunctionSupportedValue First { get; set; }

    [JsonProperty("second")]
    public IFunctionSupportedValue Second { get; set; }

    [JsonProperty("ifScript")]
    public JLioScript IfScript { get; set; }

    [JsonProperty("elseScript")]
    public JLioScript ElseScript { get; set; }

    public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
    {
        var validation = ValidateCommandInstance();
        if (!validation.IsValid)
        {
            validation.ValidationMessages.ForEach(m => context.LogWarning(CoreConstants.CommandExecution, m));
            return new JLioExecutionResult(false, dataContext);
        }

        var firstValue = First.GetValue(dataContext, dataContext, context).Data.GetJTokenValue();
        var secondValue = Second.GetValue(dataContext, dataContext, context).Data.GetJTokenValue();

        if (JToken.DeepEquals(firstValue, secondValue))
        {
            return IfScript?.Execute(dataContext, context) ?? JLioExecutionResult.SuccessFul(dataContext);
        }

        return ElseScript?.Execute(dataContext, context) ?? JLioExecutionResult.SuccessFul(dataContext);
    }

    public override ValidationResult ValidateCommandInstance()
    {
        var result = new ValidationResult();
        if (First == null)
            result.ValidationMessages.Add($"First property for {CommandName} command is missing");
        if (Second == null)
            result.ValidationMessages.Add($"Second property for {CommandName} command is missing");
        if (IfScript == null)
            result.ValidationMessages.Add($"IfScript property for {CommandName} command is missing");
        return result;
    }
}
