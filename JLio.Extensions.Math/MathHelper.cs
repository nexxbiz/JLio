using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Math;

/// <summary>
/// Helper class for math operations
/// </summary>
internal static class MathHelper
{
    /// <summary>
    /// Creates a JValue from a double, using an integer type if the value is a whole number
    /// </summary>
    /// <param name="value">The numeric value to wrap</param>
    /// <returns>A JValue containing either an integer or a double</returns>
    public static JValue CreateNumericValue(double value)
    {
        // Check if the value is a whole number (no fractional part)
        // and within the range of a long integer
        if (value == System.Math.Floor(value) && 
            !double.IsInfinity(value) && 
            !double.IsNaN(value) &&
            value >= long.MinValue && 
            value <= long.MaxValue)
        {
            // Return as integer (long) to avoid .0 suffix
            return new JValue((long)value);
        }
        
        // Return as double for non-integer values
        return new JValue(value);
    }
}
