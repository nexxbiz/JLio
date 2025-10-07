using System.Text.RegularExpressions;
using JLio.Validation.Validation;

namespace JLio.Validation.Paths;

/// <summary>
/// Basic implementation of JSONPath validation with minimal syntax checking.
/// </summary>
public class BasicJsonPathValidator : IJsonPathValidator
{
    private static readonly Regex FilterPattern = new(@"\[\?\([^)]*\)\]", RegexOptions.Compiled);

    /// <summary>
    /// Validates a JSONPath expression for basic syntax correctness.
    /// </summary>
    /// <param name="path">The JSONPath expression to validate.</param>
    /// <param name="jsonPointer">The RFC 6901 JSON Pointer to the location where this path appears.</param>
    /// <param name="commandIndex">The command index where this path appears.</param>
    /// <returns>A tuple indicating if the path is valid and any validation issues found.</returns>
    public (bool IsValid, IReadOnlyList<ValidationIssue> Issues) Validate(string path, string jsonPointer, int commandIndex)
    {
        var issues = new List<ValidationIssue>();

        // Check if path is null or empty
        if (string.IsNullOrEmpty(path))
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Path.Empty",
                "JSONPath cannot be empty",
                jsonPointer,
                commandIndex));
            return (false, issues);
        }

        // Check if path is whitespace only
        if (string.IsNullOrWhiteSpace(path))
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Path.Invalid",
                "JSONPath cannot be whitespace only",
                jsonPointer,
                commandIndex));
            return (false, issues);
        }

        // Check if path starts with $
        if (!path.StartsWith("$"))
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Path.Invalid",
                "JSONPath must start with '$'",
                jsonPointer,
                commandIndex));
            return (false, issues);
        }

        // Check for balanced brackets and parentheses
        if (!AreBracketsBalanced(path))
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Path.Invalid",
                "JSONPath has unbalanced brackets or parentheses",
                jsonPointer,
                commandIndex));
            return (false, issues);
        }

        // Check for empty segments like $.
        if (HasEmptySegments(path))
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Error,
                "Path.Invalid",
                "JSONPath has empty segments",
                jsonPointer,
                commandIndex));
            return (false, issues);
        }

        // Check for filter expressions and emit warnings
        if (FilterPattern.IsMatch(path))
        {
            issues.Add(new ValidationIssue(
                IssueSeverity.Warning,
                "Path.FilterPartialSupport",
                "JSONPath filter expressions have limited support",
                jsonPointer,
                commandIndex));
        }

        bool isValid = !issues.Any(i => i.Severity == IssueSeverity.Error);
        return (isValid, issues);
    }

    private static bool AreBracketsBalanced(string path)
    {
        int squareBrackets = 0;
        int parentheses = 0;
        bool inQuotes = false;
        char quoteChar = '\0';

        for (int i = 0; i < path.Length; i++)
        {
            char c = path[i];

            if (!inQuotes && (c == '\'' || c == '"'))
            {
                inQuotes = true;
                quoteChar = c;
            }
            else if (inQuotes && c == quoteChar)
            {
                inQuotes = false;
                quoteChar = '\0';
            }
            else if (!inQuotes)
            {
                switch (c)
                {
                    case '[':
                        squareBrackets++;
                        break;
                    case ']':
                        squareBrackets--;
                        if (squareBrackets < 0) return false;
                        break;
                    case '(':
                        parentheses++;
                        break;
                    case ')':
                        parentheses--;
                        if (parentheses < 0) return false;
                        break;
                }
            }
        }

        return squareBrackets == 0 && parentheses == 0;
    }

    private static bool HasEmptySegments(string path)
    {
        // Check for patterns like $.., $., $..property (but allow $.. as recursive descent)
        if (path.Contains("...")) return true; // More than two dots
        
        // Split on dots but be careful about recursive descent
        var segments = path.Split('.');
        for (int i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];
            
            // Skip the root $ segment
            if (i == 0 && segment == "$") continue;
            
            // Allow empty segment only if it's part of recursive descent (..)
            if (string.IsNullOrEmpty(segment))
            {
                // This is only valid if it's part of recursive descent
                // i.e., we have an empty segment between two dots
                if (i == 0 || i == segments.Length - 1) return true; // Empty at start or end is invalid
                continue;
            }
            
            // Check for segment ending with [ but no ] (incomplete bracket)
            if (segment.EndsWith("[") && !segment.Contains("]")) return true;
        }

        return false;
    }
}