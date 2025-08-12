using JLio.Commands.Logic;
using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Commands;

public class Copy : CopyMove
{
    public Copy()
    {
    }

    public Copy(string from, string to, bool destinationAsArray = false)
    {
        FromPath = from;
        ToPath = to;
        DestinationAsArray = destinationAsArray;
    }

    public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
    {
        var validationResult = ValidateCommandInstance();
        if (!validationResult.IsValid)
        {
            validationResult.ValidationMessages.ForEach(i =>
                context.LogWarning(CoreConstants.CommandExecution, i));
            return new JLioExecutionResult(false, dataContext);
        }

        return Execute(dataContext, context, EAction.Copy);
    }

    public override ValidationResult ValidateCommandInstance()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(FromPath))
            result.ValidationMessages.Add($"FromPath property for {CommandName} command is missing");

        if (string.IsNullOrWhiteSpace(ToPath))
            result.ValidationMessages.Add($"ToPath property for {CommandName} command is missing");

        return result;
    }
}