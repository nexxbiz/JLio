namespace JLio.Extensions.Math.Builders;

public static class MaxBuilders
{
    public static Max Max(params string[] arguments)
    {
        return new Max(arguments);
    }
}