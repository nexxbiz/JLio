namespace JLio.Extensions.Text.Builders;

public static class TrimEndBuilders
{
    public static TrimEnd TrimEnd(string text)
    {
        return new TrimEnd(text);
    }
    
    public static TrimEnd TrimEnd(string text, string trimChars)
    {
        return new TrimEnd(text, trimChars);
    }
}