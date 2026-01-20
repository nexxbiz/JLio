namespace JLio.Extensions.Math.Builders;

public static class SumIfsBuilders
{
    public static SumIfs SumIfs(params string[] arguments)
    {
        return new SumIfs(arguments);
    }
}
