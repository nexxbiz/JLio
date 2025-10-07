namespace JLio.Validation.Validation;

/// <summary>
/// Represents the result of JLio script validation.
/// </summary>
public sealed class ValidationResult
{
    /// <summary>
    /// Gets a value indicating whether validation was successful.
    /// Validation succeeds if there are no Error-level issues.
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    /// Gets the list of validation issues found during validation.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationResult class.
    /// </summary>
    /// <param name="issues">The list of validation issues.</param>
    public ValidationResult(IEnumerable<ValidationIssue> issues)
    {
        Issues = (issues ?? throw new ArgumentNullException(nameof(issues))).ToList();
        Success = !Issues.Any(i => i.Severity == IssueSeverity.Error);
    }

    /// <summary>
    /// Creates a successful validation result with no issues.
    /// </summary>
    /// <returns>A successful ValidationResult.</returns>
    public static ValidationResult CreateSuccess()
    {
        return new ValidationResult(Enumerable.Empty<ValidationIssue>());
    }
}