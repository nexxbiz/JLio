namespace JLio.Extensions.Text.Builders;

public static class ReplaceBuilders
{
    public static Replace Replace(string text, string oldValue, string newValue)
    {
        return new Replace(text, oldValue, newValue);
    }
    
    public static Replace Replace(string text, string oldValue, string newValue, string ignoreCase)
    {
        return new Replace(text, oldValue, newValue, ignoreCase);
    }
}