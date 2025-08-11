namespace JLio.Extensions.Text.Builders;

public static class IndexOfBuilders
{
    public static IndexOf IndexOf(string text, string searchValue)
    {
        return new IndexOf(text, searchValue);
    }
    
    public static IndexOf IndexOf(string text, string searchValue, string startIndex)
    {
        return new IndexOf(text, searchValue, startIndex);
    }
    
    public static IndexOf IndexOf(string text, string searchValue, string startIndex, string ignoreCase)
    {
        return new IndexOf(text, searchValue, startIndex, ignoreCase);
    }
}