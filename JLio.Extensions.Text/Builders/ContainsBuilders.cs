namespace JLio.Extensions.Text.Builders;

public static class ContainsBuilders
{
    public static Contains Contains(string text, string searchValue)
    {
        return new Contains(text, searchValue);
    }
    
    public static Contains Contains(string text, string searchValue, string ignoreCase)
    {
        return new Contains(text, searchValue, ignoreCase);
    }
}