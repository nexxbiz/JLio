namespace JLio.Extensions.Math.Builders;

public static class SubtractBuilders
{
    public static Subtract Subtract(params string[] arguments)
    {
        return new Subtract(arguments);
    }
}
