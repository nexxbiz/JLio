namespace JLio.Extensions.Math.Builders;

public static class AvgBuilders
{
    public static Avg Avg(params string[] arguments)
    {
        return new Avg(arguments);
    }
}
