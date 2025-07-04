﻿using System.Collections.Generic;
using System.Linq;

namespace JLio.Core.Models.Path;

public static class SplitText
{
    public static ChoppedElements GetChoppedElements(string text, char argumentsDelimiter,
        List<LevelPair> levelPairs)
    {
        return SplitOnDelimiters(text, FindDelimiterIndexes(text, new[] {argumentsDelimiter}, levelPairs),
            new[] {argumentsDelimiter});
    }

    public static ChoppedElements GetChoppedElements(string text, char[] delimiterCharacters,
        List<LevelPair> levelPairs)
    {
        return SplitOnDelimiters(text, FindDelimiterIndexes(text, delimiterCharacters, levelPairs),
            delimiterCharacters);
    }

    private static ChoppedElements SplitOnDelimiters(string text, List<DelimiterInfo> delimiterIndexes,
        char[] delimiterCharacters)
    {
        var result = new ChoppedElements();
        var previousIndex = new DelimiterInfo {Index = -1, Level = -1};
        foreach (var item in delimiterIndexes.Where(i => i.Level == 0))
        {
            result.Add(GetChoppedElement(text, previousIndex.Index, item.Index, delimiterCharacters));
            previousIndex = item;
        }

        if (previousIndex.Index < text.Length && text.Length > 0)
            if (text.StartsWith(CoreConstants.FunctionStartCharacters) &&
                delimiterCharacters.All(i => i != text.Last()))
                result.Add(GetChoppedElement(text, previousIndex.Index, delimiterIndexes.Last().Index,
                    delimiterCharacters));
            else
                result.Add(GetChoppedElement(text, previousIndex.Index, text.Length, delimiterCharacters));
        return result;
    }

    private static ChoppedElement GetChoppedElement(string text, int previousDelimiterIndex, int delimiterIndex,
        char[] delimiterCharacters)
    {
        var resultText = GetTextBetweenDelimiters(text, previousDelimiterIndex, delimiterIndex);
        if (!string.IsNullOrEmpty(resultText) &&   delimiterCharacters.Any(c => c == resultText.ToCharArray().Last()))
            return new ChoppedElement(resultText.Substring(0, resultText.Length - 1));
        return new ChoppedElement(resultText);
    }

    private static string GetTextBetweenDelimiters(string text, int previousDelimiterIndex, int delimiterIndex)
    {
        return text.Substring(previousDelimiterIndex + 1, delimiterIndex - previousDelimiterIndex - 1);
    }

    private static List<DelimiterInfo> FindDelimiterIndexes(string text, char[] delimiterCharacters,
        List<LevelPair> levelPairs)
    {
        var result = new List<DelimiterInfo>();
        var levels = new List<LevelPair>();


        for (var i = 0; i < text.Length; i++)
        {
            var character = text[i];
            if (delimiterCharacters.Contains(character))
                result.Add(new DelimiterInfo {Index = i, Level = levels.Count});
            var levelIndicator =
                levelPairs.FirstOrDefault(l => l.OpenCharacter == character || l.CloseCharacter == character);
            if (levelIndicator != null) HandleLevels(levels, character, levelIndicator);
        }

        return result;
    }

    private static void HandleLevels(List<LevelPair> levels, char character, LevelPair levelIndicator)
    {
        //when characters are not the same and it is a close character then level should be removed
        if (!levelIndicator.OpenCloseAreSameCharacter && character == levelIndicator.CloseCharacter && levels.Any() && levels.Last().SubLevelsPossible)
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