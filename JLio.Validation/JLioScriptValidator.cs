using JLio.Validation.Commands;
using JLio.Validation.Validation;
using Newtonsoft.Json.Linq;

namespace JLio.Validation;

/// <summary>
/// Validator for JLio JSON scripts with extensible command validation.
/// </summary>
public sealed class JLioScriptValidator
{
    private readonly Dictionary<string, ICommandSpec> _registry;
    private readonly JLioValidationOptions _options;

    /// <summary>
    /// Initializes a new instance of the JLioScriptValidator class.
    /// </summary>
    /// <param name="specs">The command specifications to register.</param>
    /// <param name="options">The validation options.</param>
    public JLioScriptValidator(IEnumerable<ICommandSpec> specs, JLioValidationOptions? options = null)
    {
        _registry = new Dictionary<string, ICommandSpec>();
        _options = options ?? new JLioValidationOptions();

        if (specs != null)
        {
            foreach (var spec in specs)
            {
                _registry[spec.Name] = spec;
            }
        }
    }

    /// <summary>
    /// Creates a default validator with the "add" command registered.
    /// </summary>
    /// <param name="options">The validation options.</param>
    /// <returns>A new JLioScriptValidator instance.</returns>
    public static JLioScriptValidator CreateDefault(JLioValidationOptions? options = null)
    {
        return new JLioScriptValidator(new[] { new AddCommandSpec() }, options);
    }

    /// <summary>
    /// Validates a JLio script.
    /// </summary>
    /// <param name="script">The script to validate as a JToken.</param>
    /// <returns>A ValidationResult indicating success/failure and any issues found.</returns>
    public ValidationResult Validate(JToken script)
    {
        var issues = new List<ValidationIssue>();

        // Check if script is a JArray
        if (script is not JArray scriptArray)
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Script.NotArray",
                "JLio script must be a JSON array",
                "",
                null));
            return new ValidationResult(issues);
        }

        // Validate each command in the array
        for (int i = 0; i < scriptArray.Count; i++)
        {
            var commandToken = scriptArray[i];
            
            if (commandToken is not JObject commandObject)
            {
                issues.Add(new ValidationIssue(
                    IssueSeverity.Error,
                    "Command.NotObject",
                    "Each command must be a JSON object",
                    $"/{i}",
                    i));
                continue;
            }

            ValidateCommand(commandObject, i, issues);
        }

        return new ValidationResult(issues);
    }

    private void ValidateCommand(JObject command, int index, List<ValidationIssue> issues)
    {
        // Extract command name
        var commandToken = command["command"];
        if (commandToken == null || commandToken.Type != JTokenType.String)
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Command.MissingOrInvalid",
                "Command property is missing or not a string",
                $"/{index}/command",
                index));
            return;
        }

        var commandName = commandToken.Value<string>();
        if (string.IsNullOrEmpty(commandName))
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Command.MissingOrInvalid",
                "Command property cannot be empty",
                $"/{index}/command",
                index));
            return;
        }

        // Look up command spec
        if (_registry.TryGetValue(commandName, out var spec))
        {
            // Use command-specific validation
            var commandIssues = spec.ValidateCommand(command, index, _options);
            issues.AddRange(commandIssues);
        }
        else
        {
            // Handle unknown command
            var severity = _options.AllowUnknownCommands ? IssueSeverity.Warning : IssueSeverity.Error;
            issues.Add(new ValidationIssue(
                severity,
                "Command.Unknown",
                $"Unknown command '{commandName}'",
                $"/{index}/command",
                index));

            // For unknown commands, still perform generic validation (e.g., path property)
            PerformGenericValidation(command, index, issues);
        }
    }

    private void PerformGenericValidation(JObject command, int index, List<ValidationIssue> issues)
    {
        // Generic validation for path property if present
        var pathToken = command["path"];
        if (pathToken != null)
        {
            if (pathToken.Type != JTokenType.String)
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
                if (!string.IsNullOrEmpty(pathValue))
                {
                    // Validate JSONPath syntax even for unknown commands
                    var (isValid, pathIssues) = _options.JsonPathValidator.Validate(pathValue, $"/{index}/path", index);
                    issues.AddRange(pathIssues);
                }
            }
        }
    }
}