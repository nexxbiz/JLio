using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace JLio.Commands.Logic
{
    public static class ArrayHelpers
    {
        internal static List<JToken> FindTargetArrayElementForKeys(JToken target, JToken itemToMatch, List<string> keys)
        {
            var result = new List<JToken>();
            if (keys.Any())
                target.Where(t => AllKeyMatch(t, keys, itemToMatch)).ToList()
                    .ForEach(t => result.Add(t));
            else
                target.Where(t => JToken.DeepEquals(t, itemToMatch)).ToList()
                    .ForEach(t => result.Add(t));
            return result;
        }

        internal static bool AllKeyMatch(JToken targetItem, List<string> keys, JToken itemToMatch)
        {
            var matchResult = keys.All(k =>
            {
                var result = AreTheSameValue(k, targetItem, itemToMatch);

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
            List<int> notAllowedIndexes)
        {
            var found = false;
            JToken foundItem = null;
            var foundIndex = -1;
            for (var i = 0; i < array.Count; i++)
            {
                if (notAllowedIndexes.Contains(i) || !JToken.DeepEquals(array[i], item)) continue;
                found = true;
                foundItem = array[i];
                foundIndex = i;
                break;
            }

            return (found, foundItem, foundIndex);
        }
    }
}