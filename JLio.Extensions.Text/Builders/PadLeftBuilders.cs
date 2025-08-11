namespace JLio.Extensions.Text.Builders;

public static class PadLeftBuilders
{
    public static PadLeft PadLeft(string text, string totalWidth)
    {
        return new PadLeft(text, totalWidth);
    }
    
    public static PadLeft PadLeft(string text, string totalWidth, string padChar)
    {
        return new PadLeft(text, totalWidth, padChar);
    }
}