namespace JLio.Extensions.Math.Builders;

public static class MaxIfsBuilders
{
    public static MaxIfs MaxIfs(params string[] arguments)
    {
        return new MaxIfs(arguments);
    }
}
