namespace JLio.Extensions.Text.Builders;

public static class TrimStartBuilders
{
    public static TrimStart TrimStart(string text)
    {
        return new TrimStart(text);
    }
    
    public static TrimStart TrimStart(string text, string trimChars)
    {
        return new TrimStart(text, trimChars);
    }
}