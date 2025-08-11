using JLio.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JLio.Extensions.Text;

internal static class TextHelpers
{
    /// <summary>
    /// Converts a JToken to a string value, handling JSON serialization properly
    /// This follows the same logic as the Concat function for consistency
    /// </summary>
    public static string GetStringValue(JToken token)
    {
        if (token == null || token.Type == JTokenType.Null)
            return string.Empty;
            
        // Use the same approach as Concat function
        var value = JsonConvert.SerializeObject(token);
        
        // Handle null serialization
        if (value == "null")
            return string.Empty;
            
        if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
            value = value.Substring(1, value.Length - 2);
        return value.Trim(CoreConstants.StringIndicator);
    }
    
    /// <summary>
    /// Tries to parse a JToken as an integer value
    /// </summary>
    public static bool TryGetIntegerValue(JToken token, out int value)
    {
        value = 0;
        return token.Type switch
        {
            JTokenType.Integer => (value = token.Value<int>()) >= 0 || value < 0, // Always return true for integers
            JTokenType.String => int.TryParse(GetStringValue(token), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out value),
            _ => false
        };
    }
    
    /// <summary>
    /// Tries to parse a JToken as a boolean value
    /// </summary>
    public static bool TryGetBooleanValue(JToken token, out bool value)
    {
        value = false;
        return token.Type switch
        {
            JTokenType.Boolean => (value = token.Value<bool>()) || !value, // Always return true for booleans
            JTokenType.String => bool.TryParse(GetStringValue(token), out value),
            JTokenType.Integer => (value = token.Value<int>() != 0) || !value, // 0 = false, anything else = true
            _ => false
        };
    }
}