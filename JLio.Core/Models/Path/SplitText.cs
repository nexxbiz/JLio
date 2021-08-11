using System.Collections.Generic;
using System.Linq;

namespace JLio.Core.Models.Path
{
    internal static class SplitText
    {
        internal static ChoppedElements GetChoppedElements(string text, char argumentsDelimeter,
            List<LevelPair> levelPairs)
        {
            return SplitOnDelimiters(text, FindDelimiterIndexes(text, new[] {argumentsDelimeter}, levelPairs));
        }

        private static ChoppedElements SplitOnDelimiters(string text, List<DelimiterInfo> delimiterIndexes)
        {
            var result = new ChoppedElements();
            var previousIndex = -1;
            foreach (var item in delimiterIndexes.Where(i => i.Level == 0))
            {
                result.Add(GetChoppedElement(text, previousIndex, item.Index));
                previousIndex = item.Index;
            }

            if (previousIndex < text.Length && text.Length > 0)
                result.Add(GetChoppedElement(text, previousIndex, text.Length));
            return result;
        }

        private static ChoppedElement GetChoppedElement(string text, int previousIndex, int delimiterIndex)
        {
            return new ChoppedElement(text.Substring(previousIndex + 1, delimiterIndex - previousIndex - 1));
        }

        private static List<DelimiterInfo> FindDelimiterIndexes(string text, char[] DelimiterCharacters,
            List<LevelPair> levelPairs)
        {
            var result = new List<DelimiterInfo>();
            var levels = new List<LevelPair>();


            for (var i = 0; i < text.Length; i++)
            {
                var character = text[i];
                if (DelimiterCharacters.Contains(character))
                    result.Add(new DelimiterInfo {Index = i, Level = levels.Count});
                var levelIndicator =
                    levelPairs.FirstOrDefault(l => l.OpenCharacter == character || l.CloseCharacter == character);
                if (levelIndicator != null) HandleLevels(levels, character, levelIndicator);
            }

            //todo think about how to handle this;
            //IsValidPathStructure = levels.Count == 0;
            return result;
        }

        private static void HandleLevels(List<LevelPair> levels, char character, LevelPair levelIndicator)
        {
            //when characters are not the same and it is a close character then level should be removed
            if (!levelIndicator.OpenCloseAreSameCharacter && character == levelIndicator.CloseCharacter)
            {
                if (levels.Count > 0) levels.RemoveAt(levels.Count - 1);
            }

            //when characters are the same and last level is same character then level should be removed
            else if (levelIndicator.OpenCloseAreSameCharacter && levels.Any() &&
                     character == levels.Last().CloseCharacter)
            {
                levels.RemoveAt(levels.Count - 1);
            }

            else if (!levels.Any() || levels.Any() && levels.Last().SubLevelsPossible)
            {
                levels.Add(levelIndicator);
            }
        }
    }
}