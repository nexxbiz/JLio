namespace JLio.Extensions.Math.Builders;

public static class CountBuilders
{
    public static Count Count(params string[] arguments)
    {
        return new Count(arguments);
    }
}
