namespace JLio.Extensions.Text.Builders;

public static class PadRightBuilders
{
    public static PadRight PadRight(string text, string totalWidth)
    {
        return new PadRight(text, totalWidth);
    }
    
    public static PadRight PadRight(string text, string totalWidth, string padChar)
    {
        return new PadRight(text, totalWidth, padChar);
    }
}