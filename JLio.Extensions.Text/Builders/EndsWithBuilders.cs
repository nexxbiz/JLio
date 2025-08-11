namespace JLio.Extensions.Text.Builders;

public static class EndsWithBuilders
{
    public static EndsWith EndsWith(string text, string suffix)
    {
        return new EndsWith(text, suffix);
    }
    
    public static EndsWith EndsWith(string text, string suffix, string ignoreCase)
    {
        return new EndsWith(text, suffix, ignoreCase);
    }
}