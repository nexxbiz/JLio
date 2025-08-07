using System;
using System.Text.RegularExpressions;
using JLio.Core.Contracts;
using JLio.Core.Models;
using Newtonsoft.Json.Linq;

namespace JLio.Core.Extensions;

public class JsonPathItemsFetcher : IItemsFetcher
{
    private static readonly Regex IndirectPattern = new(@"=indirect\(([^)]+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string ArrayCloseChar { get; } = "]";
    public string CurrentItemPathIndicator { get; } = "@";
    public string PathDelimiter { get; } = ".";
    public string RootPathIndicator { get; } = "$";
    public string ParentPathIndicator { get; } = "<--";

    public SelectedTokens SelectTokens(string path, JToken data)
    {
        if (data == null) 
        { 
            return new SelectedTokens(JValue.CreateNull()); 
        }

        // Check if the path contains =indirect() function
        var processedPath = ProcessIndirectPath(path, data);
        if (processedPath == null)
        {
            // Indirect path processing failed, return empty result
            return new SelectedTokens(new JToken[0]);
        }

        return new SelectedTokens(data.SelectTokens(processedPath));
    }

    public JToken SelectToken(string path, JToken data)
    {
        if (data == null)
        {
            return JValue.CreateNull();
        }

        // Check if the path contains =indirect() function
        var processedPath = ProcessIndirectPath(path, data);
        if (processedPath == null)
        {
            // Indirect path processing failed, return null
            return null;
        }

        return data.SelectToken(processedPath);
    }

    public string GetPath(JToken token)
    {
        if (token?.Path == null || string.IsNullOrEmpty(token.Path))
        {
            return RootPathIndicator;
        }

        return $"{RootPathIndicator}{PathDelimiter}{token.Path}";
    }

    public JToken NavigateToParent(JToken currentToken, int levels = 1)
    {
        if (currentToken == null || levels <= 0)
            return currentToken;

        var targetToken = currentToken;
        
        for (int i = 0; i < levels && targetToken != null; i++)
        {
            targetToken = NavigateToSemanticParent(targetToken);
        }
        
        return targetToken;
    }

    public string ResolveRelativePath(string relativePath, JToken currentToken, JToken dataContext)
    {
        if (string.IsNullOrEmpty(relativePath))
            return GetPath(currentToken);

        // Handle @ (current item reference)
        if (relativePath.StartsWith(CurrentItemPathIndicator))
        {
            var pathAfterCurrent = relativePath.Substring(CurrentItemPathIndicator.Length);
            
            // Handle parent navigation (@.<--, @.<--.<--, etc.)
            if (pathAfterCurrent.StartsWith(PathDelimiter + ParentPathIndicator, StringComparison.InvariantCulture))
            {
                return HandleParentNavigation(pathAfterCurrent, currentToken);
            }

            // Handle regular relative path (@.property)
            var currentPath = GetPath(currentToken);
            if (string.IsNullOrEmpty(pathAfterCurrent))
            {
                return currentPath;
            }

            // Remove leading dot if present
            if (pathAfterCurrent.StartsWith(PathDelimiter))
            {
                pathAfterCurrent = pathAfterCurrent.Substring(PathDelimiter.Length);
            }

            return $"{currentPath}{PathDelimiter}{pathAfterCurrent}";
        }

        // For absolute paths, just return them as-is
        return relativePath;
    }

    private string HandleParentNavigation(string pathAfterCurrent, JToken currentToken)
    {
        // Remove the leading delimiter if present
        if (pathAfterCurrent.StartsWith(PathDelimiter))
        {
            pathAfterCurrent = pathAfterCurrent.Substring(PathDelimiter.Length);
        }

        var parentIndicatorWithDelimiter = ParentPathIndicator + PathDelimiter;
        
        // Count parent levels and extract remainder path
        var parentLevels = 0;
        var remainderPath = pathAfterCurrent;
        
        while (remainderPath.StartsWith(parentIndicatorWithDelimiter, StringComparison.InvariantCulture))
        {
            parentLevels++;
            remainderPath = remainderPath.Substring(parentIndicatorWithDelimiter.Length);
        }
        
        // Check if the path ends with a parent indicator (no delimiter after)
        if (remainderPath == ParentPathIndicator)
        {
            parentLevels++;
            remainderPath = "";
        }

        // Navigate up the parent hierarchy
        var targetToken = NavigateToParent(currentToken, parentLevels);
        
        // If we couldn't navigate up enough levels, return current path
        if (targetToken == null)
        {
            return GetPath(currentToken);
        }

        // Get the path of the target token
        var targetPath = GetPath(targetToken);

        // If there's a remainder path, append it
        if (!string.IsNullOrEmpty(remainderPath))
        {
            return $"{targetPath}{PathDelimiter}{remainderPath}";
        }
        
        return targetPath;
    }

    private JToken NavigateToSemanticParent(JToken token)
    {
        if (token?.Parent == null)
            return null;

        var parent = token.Parent;
        
        // If we're an array element, the parent is the array
        // If the array is a property value, we need to go to the object containing the property
        if (token.Parent is JArray)
        {
            // Parent is an array, check if this array is a property value
            if (parent.Parent is JProperty)
            {
                // Skip the JProperty and go to the containing object
                return parent.Parent.Parent;
            }
            // Array is not a property value (root array), return the array itself
            return parent;
        }
        
        // If we're a property value (object/primitive), the semantic parent is the object containing the property
        if (token.Parent is JProperty property)
        {
            return property.Parent;
        }
        
        // For other cases, just return the direct parent
        return parent;
    }

    private string ProcessIndirectPath(string path, JToken data)
    {
        if (!path.Contains("=indirect("))
        {
            // No indirect function, return original path
            return path;
        }

        var match = IndirectPattern.Match(path);
        if (!match.Success)
        {
            // Malformed indirect function, return null to indicate error
            return null;
        }

        try
        {
            var indirectPathReference = match.Groups[1].Value.Trim();
            
            // Remove quotes if present (e.g., '$.myPath' or "$.myPath")
            if ((indirectPathReference.StartsWith("'") && indirectPathReference.EndsWith("'")) ||
                (indirectPathReference.StartsWith("\"") && indirectPathReference.EndsWith("\"")))
            {
                indirectPathReference = indirectPathReference.Substring(1, indirectPathReference.Length - 2);
            }

            // Get the value at the indirect path reference
            var indirectPathToken = data.SelectToken(indirectPathReference);
            if (indirectPathToken == null)
            {
                // Path reference doesn't exist
                return null;
            }

            // The value should be a string containing the actual path
            if (indirectPathToken.Type != JTokenType.String)
            {
                // Indirect path reference doesn't point to a string
                return null;
            }

            var actualPath = indirectPathToken.Value<string>();
            if (string.IsNullOrEmpty(actualPath))
            {
                // Empty path string
                return null;
            }

            // Replace the =indirect(...) part with the actual path
            return IndirectPattern.Replace(path, actualPath);
        }
        catch
        {
            // Error processing indirect path
            return null;
        }
    }
}