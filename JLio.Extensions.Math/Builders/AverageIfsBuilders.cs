namespace JLio.Extensions.Math.Builders;

public static class AverageIfsBuilders
{
    public static AverageIfs AverageIfs(params string[] arguments)
    {
        return new AverageIfs(arguments);
    }
}
