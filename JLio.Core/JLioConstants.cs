using System.Collections.Generic;
using JLio.Core.Models.Path;

namespace JLio.Core
{
    public static class JLioConstants
    {
        public const char ArgumentsDelimeter = ',';
        public const string CommandDiscriminator = "command";
        public const string CommandExecution = "Command execution";
        public const string FunctionArgumentsEndCharacters = ")";
        public const string FunctionArgumentsStartCharacters = "(";
        public const string FunctionStartCharacters = "=";

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