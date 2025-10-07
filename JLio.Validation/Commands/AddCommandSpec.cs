using JLio.Validation.Validation;
using Newtonsoft.Json.Linq;

namespace JLio.Validation.Commands;

/// <summary>
/// Command specification for the "add" command.
/// </summary>
public sealed class AddCommandSpec : ICommandSpec
{
    /// <summary>
    /// Gets the name of the command this spec validates.
    /// </summary>
    public string Name => "add";

    /// <summary>
    /// Validates an "add" command object.
    /// </summary>
    /// <param name="command">The command object to validate.</param>
    /// <param name="index">The zero-based index of the command in the script array.</param>
    /// <param name="options">The validation options.</param>
    /// <returns>An enumerable of validation issues found.</returns>
    public IEnumerable<ValidationIssue> ValidateCommand(JObject command, int index, JLioValidationOptions options)
    {
        var issues = new List<ValidationIssue>();

        // Validate command property
        var commandToken = command["command"];
        if (commandToken == null || commandToken.Type != JTokenType.String || commandToken.Value<string>() != "add")
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Command.Invalid",
                "Command property must be 'add'",
                $"/{index}/command",
                index));
        }

        // Validate path property
        var pathToken = command["path"];
        if (pathToken == null)
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Prop.Missing",
                "Required property 'path' is missing",
                $"/{index}/path",
                index));
        }
        else if (pathToken.Type != JTokenType.String)
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Prop.InvalidType",
                "Property 'path' must be a string",
                $"/{index}/path",
                index));
        }
        else
        {
            var pathValue = pathToken.Value<string>();
            if (string.IsNullOrEmpty(pathValue))
            {
                issues.Add(new ValidationIssue(
                    IssueSeverity.Error,
                    "Path.Empty",
                    "Property 'path' cannot be empty",
                    $"/{index}/path",
                    index));
            }
            else
            {
                // Validate JSONPath syntax
                var (isValid, pathIssues) = options.JsonPathValidator.Validate(pathValue, $"/{index}/path", index);
                issues.AddRange(pathIssues);
            }
        }

        // Validate value property
        var valueToken = command["value"];
        if (valueToken == null)
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Prop.Missing",
                "Required property 'value' is missing",
                $"/{index}/value",
                index));
        }
        else
        {
            // If value is a string starting with =, validate as function expression
            if (valueToken.Type == JTokenType.String)
            {
                var valueStr = valueToken.Value<string>();
                if (valueStr != null && valueStr.StartsWith("="))
                {
                    ValidateFunctionExpression(valueStr, $"/{index}/value", index, issues);
                }
            }
        }

        return issues;
    }

    private static void ValidateFunctionExpression(string expression, string jsonPointer, int commandIndex, List<ValidationIssue> issues)
    {
        // Basic function expression validation - must start with = and have content after
        if (expression.Length <= 1)
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Function.Invalid",
                "Function expression cannot be empty after '='",
                jsonPointer,
                commandIndex));
            return;
        }

        // Check if it's just "= " (equals followed by whitespace)
        var afterEquals = expression.Substring(1);
        if (string.IsNullOrWhiteSpace(afterEquals))
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Function.Invalid",
                "Function expression cannot be whitespace only after '='",
                jsonPointer,
                commandIndex));
        }
    }
}