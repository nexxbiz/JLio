namespace JLio.Functions.Builders;

public static class SumBuilders
{
    public static Sum Sum(params string[] arguments)
    {
        return new Sum(arguments);
    }
}
