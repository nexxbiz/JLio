namespace JLio.Extensions.Text.Builders;

public static class ToLowerBuilders
{
    public static ToLower ToLower(string text)
    {
        return new ToLower(text);
    }
    
    public static ToLower ToLower(string text, string culture)
    {
        return new ToLower(text, culture);
    }
}