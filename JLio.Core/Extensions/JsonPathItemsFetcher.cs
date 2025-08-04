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

            if (indirectPathToken.Type != JTokenType.String)
            {
                // Path reference doesn't resolve to a string
                return null;
            }

            var actualPath = indirectPathToken.Value<string>();
            if (string.IsNullOrWhiteSpace(actualPath))
            {
                // Path reference resolves to empty string
                return null;
            }

            // Replace the =indirect() part with the actual path
            var result = IndirectPattern.Replace(path, actualPath);
            
            // Recursively process in case there are nested indirects
            return ProcessIndirectPath(result, data);
        }
        catch
        {
            // Any error in processing means invalid path
            return null;
        }
    }

    public string GetPath(string path, JToken dataContext)
    {
        if (dataContext == null)
        {
            return path;
        }

        if (!path.Contains("=indirect("))
        {
            // No indirect function, return original path
            return path;
        }

        var match = IndirectPattern.Match(path);
        if (!match.Success)
        {
            // Malformed indirect function, return original path
            return path;
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
            var indirectPathToken = dataContext.SelectToken(indirectPathReference);
            if (indirectPathToken == null || indirectPathToken.Type != JTokenType.String)
            {
                // Path reference doesn't exist or doesn't resolve to a string
                return path;
            }

            var actualPath = indirectPathToken.Value<string>();
            if (string.IsNullOrWhiteSpace(actualPath))
            {
                // Path reference resolves to empty string
                return path;
            }

            // Replace the =indirect() part with the actual path
            var result = IndirectPattern.Replace(path, actualPath);
            
            // Recursively process in case there are nested indirects
            return GetPath(result, dataContext);
        }
        catch
        {
            // Any error in processing means return original path
            return path;
        }
    }
}