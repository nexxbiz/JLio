using System.Collections.Generic;
using System.Linq;
using JLio.Commands.Advanced.Settings;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Logic;

public static class ArrayHelpers
{
    internal static List<JToken> FindTargetArrayElementForKeys(JToken item, JToken itemToMatch, List<string> keys)
    {
        var result = new List<JToken>();
        if (keys.Any())
            item.Where(t => AllKeyMatch(t, keys, itemToMatch)).ToList()
                .ForEach(t => result.Add(t));
        else
            item.Where(t => JToken.DeepEquals(t, itemToMatch)).ToList()
                .ForEach(t => result.Add(t));
        return result;
    }

    internal static bool AllKeyMatch(JToken item, List<string> keys, JToken itemToMatch)
    {
        var matchResult = keys.All(k =>
        {
            var result = AreTheSameValue(k, item, itemToMatch);

            return result;
        });
        return matchResult;
    }

    /// <summary>
    /// Compares values from two tokens using the specified JSON path.
    /// Supports both @ notation (@.property) and plain notation (property) for consistency with JLio framework.
    /// </summary>
    /// <param name="jsonPath">
    /// The path to the property to compare. Can be:
    /// - Plain notation: "id", "name", "key.id" 
    /// - @ notation: "@.id", "@.name", "@.key.id" (for consistency with other JLio functions)
    /// - Empty string: compares the tokens directly
    /// </param>
    /// <param name="firstElement">The first token to compare</param>
    /// <param name="secondElement">The second token to compare</param>
    /// <returns>True if the values at the specified path are equal, false otherwise</returns>
    private static bool AreTheSameValue(string jsonPath, JToken firstElement, JToken secondElement)
    {
        // Handle both @ notation and plain property notation for consistency
        string normalizedPath;
        if (jsonPath.StartsWith("@.") || jsonPath.StartsWith("@"))
        {
            // If the path starts with @, it's already relative to the current item
            // Just remove the @ and we'll use it directly on the tokens
            normalizedPath = jsonPath.StartsWith("@.") ? jsonPath.Substring(2) : jsonPath.Substring(1);
            if (normalizedPath.StartsWith("."))
            {
                normalizedPath = normalizedPath.Substring(1);
            }
        }
        else
        {
            // For backward compatibility, support paths without @ 
            normalizedPath = jsonPath;
        }

        // If normalized path is empty, compare the tokens directly
        if (string.IsNullOrEmpty(normalizedPath))
        {
            return JToken.DeepEquals(firstElement, secondElement);
        }

        // Add the root indicator if needed
        if (!normalizedPath.StartsWith("$"))
        {
            normalizedPath = $"$.{normalizedPath}";
        }

        var firstKeyValue = firstElement.SelectToken(normalizedPath);
        var secondKeyValue = secondElement.SelectToken(normalizedPath);
        return JToken.DeepEquals(firstKeyValue, secondKeyValue);
    }

    internal static bool IsItemInArray(JArray array, JToken item)
    {
        return array.Any(t => JToken.DeepEquals(t, item));
    }

    internal static (bool Found, JToken Item, int Index) FindInArray(JArray array, JToken item,
        List<int> notAllowedIndexes, CompareArraySettings settings)
    {
        var found = false;
        JToken foundItem = null;
        var foundIndex = -1;
        for (var i = 0; i < array.Count; i++)
        {
            if (notAllowedIndexes.Contains(i) || !AreTheSame(array[i], item, settings?.KeyPaths)) continue;
            found = true;
            foundItem = array[i];
            foundIndex = i;
            break;
        }

        return (found, foundItem, foundIndex);
    }

    private static bool AreTheSame(JToken item, JToken itemToMatch, List<string> keys)
    {
        if (keys != null && keys.Count > 0 && item.Type == JTokenType.Object &&
            itemToMatch.Type == JTokenType.Object) return AllKeyMatch(item, keys, itemToMatch);
        if (item.Type is JTokenType.Array && itemToMatch.Type == JTokenType.Array)
            return ArrayCompareWithOrderingForPrimitiveComparison(item, itemToMatch);

        return JToken.DeepEquals(item, itemToMatch);
    }

    private static bool ArrayCompareWithOrderingForPrimitiveComparison(JToken item, JToken itemToMatch)
    {
        var first = new JArray(((JArray) item).OrderBy(i => i));
        var Second = new JArray(((JArray) itemToMatch).OrderBy(i => i));
        return JToken.DeepEquals(first, Second);
    }
}