namespace JLio.Core.Models.Path;

public class LevelPair
{
    public char CloseCharacter { get; set; }
    public char OpenCharacter { get; set; }
    public bool OpenCloseAreSameCharacter => OpenCharacter == CloseCharacter;
    public bool SubLevelsPossible { get; set; } = true;
}