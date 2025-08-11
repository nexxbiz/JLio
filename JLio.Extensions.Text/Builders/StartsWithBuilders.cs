namespace JLio.Extensions.Text.Builders;

public static class StartsWithBuilders
{
    public static StartsWith StartsWith(string text, string prefix)
    {
        return new StartsWith(text, prefix);
    }
    
    public static StartsWith StartsWith(string text, string prefix, string ignoreCase)
    {
        return new StartsWith(text, prefix, ignoreCase);
    }
}