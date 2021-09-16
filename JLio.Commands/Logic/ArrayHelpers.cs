using System.Collections.Generic;
using System.Linq;
using JLio.Commands.Advanced.Settings;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Logic
{
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

        private static bool AreTheSameValue(string jsonPath, JToken firstElement, JToken secondElement)
        {
            jsonPath = $"$.{jsonPath}";
            var firstKeyValue = firstElement.SelectToken(jsonPath);
            var secondKeyValue = secondElement.SelectToken(jsonPath);
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
}