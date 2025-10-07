using JLio.Validation.Paths;

namespace JLio.Validation;

/// <summary>
/// Options for configuring JLio script validation.
/// </summary>
public sealed class JLioValidationOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether unknown commands should be allowed.
    /// When true, unknown commands generate warnings but don't fail validation.
    /// When false, unknown commands generate errors and fail validation.
    /// </summary>
    public bool AllowUnknownCommands { get; init; } = false;

    /// <summary>
    /// Gets or sets the JSONPath validator to use for validating path expressions.
    /// </summary>
    public IJsonPathValidator JsonPathValidator { get; init; } = new BasicJsonPathValidator();
}