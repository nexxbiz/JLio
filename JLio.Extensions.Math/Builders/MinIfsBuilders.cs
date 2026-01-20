namespace JLio.Extensions.Math.Builders;

public static class MinIfsBuilders
{
    public static MinIfs MinIfs(params string[] arguments)
    {
        return new MinIfs(arguments);
    }
}
