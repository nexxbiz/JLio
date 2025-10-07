using JLio.Validation.Validation;

namespace JLio.Validation.Paths;

/// <summary>
/// Interface for validating JSONPath expressions.
/// </summary>
public interface IJsonPathValidator
{
    /// <summary>
    /// Validates a JSONPath expression for basic syntax correctness.
    /// </summary>
    /// <param name="path">The JSONPath expression to validate.</param>
    /// <param name="jsonPointer">The RFC 6901 JSON Pointer to the location where this path appears.</param>
    /// <param name="commandIndex">The command index where this path appears.</param>
    /// <returns>A tuple indicating if the path is valid and any validation issues found.</returns>
    (bool IsValid, IReadOnlyList<ValidationIssue> Issues) Validate(string path, string jsonPointer, int commandIndex);
}