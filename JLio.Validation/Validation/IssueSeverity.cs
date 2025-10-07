namespace JLio.Validation.Validation;

/// <summary>
/// Severity level for validation issues.
/// </summary>
public enum IssueSeverity
{
    /// <summary>
    /// Warning that doesn't prevent validation success.
    /// </summary>
    Warning,
    
    /// <summary>
    /// Error that causes validation failure.
    /// </summary>
    Error
}