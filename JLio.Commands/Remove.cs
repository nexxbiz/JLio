using JLio.Core;
using JLio.Core.Contracts;
using JLio.Core.Extensions;
using JLio.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Commands;

public class Remove : CommandBase
{
    private IExecutionContext executionContext;

    public Remove()
    {
    }

    public Remove(string path)
    {
        Path = path;
    }

    [JsonProperty("path")]
    public string Path { get; set; }

    public override JLioExecutionResult Execute(JToken dataContext, IExecutionContext context)
    {
        executionContext = context;
        var validationResult = ValidateCommandInstance();
        if (!validationResult.IsValid)
        {
            validationResult.ValidationMessages.ForEach(i =>
                context.LogWarning(CoreConstants.CommandExecution, i));
            return new JLioExecutionResult(false, dataContext);
        }

        RemoveItems(dataContext);

        context.LogInfo(CoreConstants.CommandExecution,
            $"{CommandName}: completed for {Path}");

        return new JLioExecutionResult(true, dataContext);
    }

    public override ValidationResult ValidateCommandInstance()
    {
        var result = new ValidationResult();
        if (string.IsNullOrWhiteSpace(Path))
            result.ValidationMessages.Add($"Path property for {CommandName} command is missing");

        return result;
    }

    private void RemoveItems(JToken data)
    {
        var targetItems =
            executionContext.ItemsFetcher.SelectTokens(Path, data);
        if (targetItems.Count == 0)
            executionContext.LogWarning(CoreConstants.CommandExecution,
                $"{Path} did not retrieve any items");
        targetItems.ForEach(i =>
        {
            var success = JsonMethods.RemoveItemFromTarget(i);
            if (!success)
                executionContext.LogError(CoreConstants.CommandExecution,
                    $"{CommandName} only possible on properties or array items");
        });
    }
}