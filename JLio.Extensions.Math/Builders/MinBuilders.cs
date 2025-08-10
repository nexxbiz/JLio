namespace JLio.Extensions.Math.Builders;

public static class MinBuilders
{
    public static Min Min(params string[] arguments)
    {
        return new Min(arguments);
    }
}