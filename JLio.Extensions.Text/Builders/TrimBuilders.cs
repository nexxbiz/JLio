namespace JLio.Extensions.Text.Builders;

public static class TrimBuilders
{
    public static Trim Trim(string text)
    {
        return new Trim(text);
    }
    
    public static Trim Trim(string text, string trimChars)
    {
        return new Trim(text, trimChars);
    }
}