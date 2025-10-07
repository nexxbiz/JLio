namespace JLio.Validation.Validation;

/// <summary>
/// Represents a validation issue found during JLio script validation.
/// </summary>
public sealed class ValidationIssue
{
    /// <summary>
    /// Gets the severity of the issue.
    /// </summary>
    public IssueSeverity Severity { get; }
    
    /// <summary>
    /// Gets a stable code identifying the type of issue.
    /// </summary>
    public string Code { get; }
    
    /// <summary>
    /// Gets a human-readable message describing the issue.
    /// </summary>
    public string Message { get; }
    
    /// <summary>
    /// Gets the RFC 6901 JSON Pointer to the location in the script where the issue occurred.
    /// </summary>
    public string JsonPointer { get; }
    
    /// <summary>
    /// Gets the zero-based index of the command in the script array where the issue occurred, if applicable.
    /// </summary>
    public int? CommandIndex { get; }

    /// <summary>
    /// Initializes a new instance of the ValidationIssue class.
    /// </summary>
    /// <param name="severity">The severity of the issue.</param>
    /// <param name="code">A stable code identifying the type of issue.</param>
    /// <param name="message">A human-readable message describing the issue.</param>
    /// <param name="jsonPointer">The RFC 6901 JSON Pointer to the location in the script.</param>
    /// <param name="commandIndex">The zero-based index of the command, if applicable.</param>
    public ValidationIssue(IssueSeverity severity, string code, string message, string jsonPointer, int? commandIndex)
    {
        Severity = severity;
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        JsonPointer = jsonPointer ?? throw new ArgumentNullException(nameof(jsonPointer));
        CommandIndex = commandIndex;
    }
}