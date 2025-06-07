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

    [JsonProperty("condition")]
    public IFunctionSupportedValue Condition { get; set; }

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
        if (Condition != null)
        {
            var conditionValue = Condition.GetValue(dataContext, dataContext, context).Data.GetJTokenValue();
            if (conditionValue.Type != JTokenType.Boolean)
            {
                context.LogWarning(CoreConstants.CommandExecution,
                    $"Condition for {CommandName} should evaluate to boolean");
                return new JLioExecutionResult(false, dataContext);
            }

            if (conditionValue.Value<bool>())
                return IfScript?.Execute(dataContext, context) ?? JLioExecutionResult.SuccessFul(dataContext);

            return ElseScript?.Execute(dataContext, context) ?? JLioExecutionResult.SuccessFul(dataContext);
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
        if (Condition == null)
        {
            if (First == null)
                result.ValidationMessages.Add($"First property for {CommandName} command is missing");
            if (Second == null)
                result.ValidationMessages.Add($"Second property for {CommandName} command is missing");
        }
        if (IfScript == null)
            result.ValidationMessages.Add($"IfScript property for {CommandName} command is missing");
        return result;
    }
}
