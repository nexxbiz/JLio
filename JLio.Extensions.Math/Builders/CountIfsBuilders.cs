namespace JLio.Extensions.Math.Builders;

public static class CountIfsBuilders
{
    public static CountIfs CountIfs(params string[] arguments)
    {
        return new CountIfs(arguments);
    }
}
