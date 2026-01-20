namespace JLio.Extensions.Math.Builders;

public static class CountIfBuilders
{
    public static CountIf CountIf(params string[] arguments)
    {
        return new CountIf(arguments);
    }
}
