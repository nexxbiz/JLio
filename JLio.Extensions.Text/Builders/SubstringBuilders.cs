namespace JLio.Extensions.Text.Builders;

public static class SubstringBuilders
{
    public static Substring Substring(string text, string startIndex)
    {
        return new Substring(text, startIndex);
    }
    
    public static Substring Substring(string text, string startIndex, string length)
    {
        return new Substring(text, startIndex, length);
    }
}