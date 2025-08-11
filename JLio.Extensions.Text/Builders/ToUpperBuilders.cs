namespace JLio.Extensions.Text.Builders;

public static class ToUpperBuilders
{
    public static ToUpper ToUpper(string text)
    {
        return new ToUpper(text);
    }
    
    public static ToUpper ToUpper(string text, string culture)
    {
        return new ToUpper(text, culture);
    }
}