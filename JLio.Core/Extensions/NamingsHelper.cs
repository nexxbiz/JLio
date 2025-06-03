namespace JLio.Core.Extensions;

public static class NamingsHelper
{
    public static string CamelCasing(this string source)
    {
        if (string.IsNullOrEmpty(source) || char.IsLower(source[0]))
            return source;
        return char.ToLower(source[0]) + source.Substring(1);
    }
}