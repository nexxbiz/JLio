namespace JLio.Functions.Builders;

public static class FetchBuilders
{
    public static Fetch Fetch(string path)
    {
        return new Fetch(path);
    }
}
