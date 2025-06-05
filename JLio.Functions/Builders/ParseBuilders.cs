namespace JLio.Functions.Builders;

public static class ParseBuilders
{
    public static Parse Parse(string path)
    {
        return new Parse(path);
    }

    public static Parse Parse()
    {
        return new Parse();
    }
}
