using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace JLio.Core;

/// <summary>
/// Shared condition evaluator that can be used by DecisionTable and conditional aggregate functions.
/// Supports comparison operators, wildcards, and type coercion.
/// </summary>
public class ConditionEvaluator
{
    /// <summary>
    /// Evaluates a condition against a value.
    /// </summary>
    /// <param name="value">The value to test</param>
    /// <param name="condition">The condition string (e.g., ">5", "apple", "*test")</param>
    /// <returns>True if the value matches the condition</returns>
    public static bool EvaluateCondition(object value, string condition)
    {
        if (string.IsNullOrEmpty(condition))
            return false;

        condition = condition.Trim();

        // Handle comparison operators (order matters - check >= before >)
        if (condition.StartsWith(">="))
            return CompareValues(value, condition.Substring(2).Trim(), ">=");
        if (condition.StartsWith("<="))
            return CompareValues(value, condition.Substring(2).Trim(), "<=");
        if (condition.StartsWith("<>"))
            return !AreEqual(value, condition.Substring(2).Trim());
        if (condition.StartsWith(">"))
            return CompareValues(value, condition.Substring(1).Trim(), ">");
        if (condition.StartsWith("<"))
            return CompareValues(value, condition.Substring(1).Trim(), "<");
        if (condition.StartsWith("="))
            return AreEqual(value, condition.Substring(1).Trim());

        // No operator - check for wildcards or direct equality
        if (condition.Contains("*") || condition.Contains("?"))
            return MatchWildcard(value?.ToString() ?? "", condition);

        // Direct equality check
        return AreEqual(value, condition);
    }

    /// <summary>
    /// Evaluates a JToken condition against a value.
    /// </summary>
    public static bool EvaluateCondition(object value, JToken conditionToken)
    {
        if (conditionToken.Type == JTokenType.String)
        {
            return EvaluateCondition(value, conditionToken.ToString());
        }

        // Direct equality check for non-string tokens
        return AreEqual(value, conditionToken.ToObject<object>());
    }

    private static bool CompareValues(object inputValue, string conditionValue, string op)
    {
        try
        {
            var inputDecimal = ParseDecimalResilient(inputValue);
            var conditionDecimal = ParseDecimalResilient(conditionValue);

            if (inputDecimal.HasValue && conditionDecimal.HasValue)
            {
                return op switch
                {
                    ">=" => inputDecimal.Value >= conditionDecimal.Value,
                    "<=" => inputDecimal.Value <= conditionDecimal.Value,
                    ">" => inputDecimal.Value > conditionDecimal.Value,
                    "<" => inputDecimal.Value < conditionDecimal.Value,
                    _ => false
                };
            }

            // Fall back to string comparison if either value can't be parsed as decimal
            var stringComparison = string.Compare(inputValue?.ToString(), conditionValue, StringComparison.OrdinalIgnoreCase);
            return op switch
            {
                ">=" => stringComparison >= 0,
                "<=" => stringComparison <= 0,
                ">" => stringComparison > 0,
                "<" => stringComparison < 0,
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }

    private static decimal? ParseDecimalResilient(object value)
    {
        if (value == null)
            return null;

        // If it's already a numeric type, convert directly
        if (value is decimal d) return d;
        if (value is double db) return (decimal)db;
        if (value is float f) return (decimal)f;
        if (value is int i) return i;
        if (value is long l) return l;

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
            return null;

        // Try multiple parsing strategies
        return TryParseWithInvariantCulture(stringValue) ??
               TryParseWithCurrentCulture(stringValue) ??
               TryParseWithNormalizedString(stringValue) ??
               TryParseWithBothSeparators(stringValue);
    }

    private static decimal? TryParseWithInvariantCulture(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }

    private static decimal? TryParseWithCurrentCulture(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var result))
            return result;
        return null;
    }

    private static decimal? TryParseWithNormalizedString(string value)
    {
        // Normalize the string by replacing culture-specific decimal separator with invariant (dot)
        var currentDecimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var normalizedValue = value.Replace(currentDecimalSeparator, ".");

        if (decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            return result;
        return null;
    }

    private static decimal? TryParseWithBothSeparators(string value)
    {
        // Try treating comma as decimal separator
        var withCommaAsDecimal = value.Replace(".", ",");
        if (decimal.TryParse(withCommaAsDecimal, NumberStyles.Number, new CultureInfo("de-DE"), out var result1))
            return result1;

        // Try treating dot as decimal separator  
        var withDotAsDecimal = value.Replace(",", ".");
        if (decimal.TryParse(withDotAsDecimal, NumberStyles.Number, CultureInfo.InvariantCulture, out var result2))
            return result2;

        return null;
    }

    private static bool AreEqual(object inputValue, object conditionValue)
    {
        if (inputValue == null && conditionValue == null) return true;
        if (inputValue == null || conditionValue == null) return false;

        return inputValue.ToString().Equals(conditionValue.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Matches a string against a wildcard pattern.
    /// * matches any sequence of characters
    /// ? matches any single character
    /// ~* and ~? escape literal * and ? characters
    /// </summary>
    private static bool MatchWildcard(string text, string pattern)
    {
        // Handle escaped wildcards by temporarily replacing them
        var escapedStar = Guid.NewGuid().ToString();
        var escapedQuestion = Guid.NewGuid().ToString();
        
        pattern = pattern.Replace("~*", escapedStar).Replace("~?", escapedQuestion);
        
        // Convert wildcard pattern to regex
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*", ".*")
            .Replace("\\?", ".") + "$";
        
        // Restore escaped wildcards as literals
        regexPattern = regexPattern.Replace(escapedStar, "\\*").Replace(escapedQuestion, "\\?");
        
        return Regex.IsMatch(text, regexPattern, RegexOptions.IgnoreCase);
    }
}
