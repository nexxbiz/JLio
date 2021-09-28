using System.Collections.Generic;
using JLio.Core.Models.Path;

namespace JLio.Core
{
    public static class CoreConstants
    {
        public const char ArgumentsDelimiter = ',';
        public const string CommandDiscriminator = "command";
        public const string CommandExecution = "Command execution";
        public const char FunctionArgumentsEndCharacters = ')';
        public const char FunctionArgumentsStartCharacters = '(';
        public const string FunctionStartCharacters = "=";
        public const string FunctionExecution = "Function execution";
        public const char StringIndicator = '\'';

        public static List<LevelPair> ArgumentLevelPairs { get; } = new List<LevelPair>
        {
            new LevelPair
            {
                OpenCharacter = '{',
                CloseCharacter = '}',
                SubLevelsPossible = true
            },
            new LevelPair
            {
                OpenCharacter = '[',
                CloseCharacter = ']',
                SubLevelsPossible = true
            },
            new LevelPair
            {
                OpenCharacter = '(',
                CloseCharacter = ')',
                SubLevelsPossible = true
            },
            new LevelPair
            {
                OpenCharacter = '"',
                CloseCharacter = '"',
                SubLevelsPossible = false
            },
            new LevelPair
            {
                OpenCharacter = '\'',
                CloseCharacter = '\'',
                SubLevelsPossible = false
            }
        };
    }
}