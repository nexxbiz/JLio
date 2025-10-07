namespace JLio.Validation.Commands;

/// <summary>
/// Context information for command validation.
/// </summary>
public sealed class CommandContext
{
    /// <summary>
    /// Gets the zero-based index of the command in the script array.
    /// </summary>
    public int Index { get; }
    
    /// <summary>
    /// Gets the validation options being used.
    /// </summary>
    public JLioValidationOptions Options { get; }

    /// <summary>
    /// Initializes a new instance of the CommandContext class.
    /// </summary>
    /// <param name="index">The zero-based index of the command.</param>
    /// <param name="options">The validation options.</param>
    public CommandContext(int index, JLioValidationOptions options)
    {
        Index = index;
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }
}