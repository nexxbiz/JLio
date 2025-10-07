using JLio.Validation.Validation;
using Newtonsoft.Json.Linq;

namespace JLio.Validation.Commands;

/// <summary>
/// Interface for validating specific command types in JLio scripts.
/// </summary>
public interface ICommandSpec
{
    /// <summary>
    /// Gets the name of the command this spec validates.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Validates a command object for this specific command type.
    /// </summary>
    /// <param name="command">The command object to validate.</param>
    /// <param name="index">The zero-based index of the command in the script array.</param>
    /// <param name="options">The validation options.</param>
    /// <returns>An enumerable of validation issues found.</returns>
    IEnumerable<ValidationIssue> ValidateCommand(JObject command, int index, JLioValidationOptions options);
}