namespace JLio.Extensions.Text.Builders;

public static class SplitBuilders
{
    public static Split Split(string text, string separator)
    {
        return new Split(text, separator);
    }
    
    public static Split Split(string text, string separator, string maxSplits)
    {
        return new Split(text, separator, maxSplits);
    }
    
    public static Split Split(string text, string separator, string maxSplits, string removeEmpty)
    {
        return new Split(text, separator, maxSplits, removeEmpty);
    }
}