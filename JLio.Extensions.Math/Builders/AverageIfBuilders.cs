namespace JLio.Extensions.Math.Builders;

public static class AverageIfBuilders
{
    public static AverageIf AverageIf(params string[] arguments)
    {
        return new AverageIf(arguments);
    }
}
