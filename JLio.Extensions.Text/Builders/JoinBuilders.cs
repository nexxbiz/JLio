namespace JLio.Extensions.Text.Builders;

public static class JoinBuilders
{
    public static Join Join(string separator, params string[] values)
    {
        var args = new string[values.Length + 1];
        args[0] = separator;
        Array.Copy(values, 0, args, 1, values.Length);
        return new Join(args);
    }
    
    public static Join Join(string separator, string array)
    {
        return new Join(separator, array);
    }
}