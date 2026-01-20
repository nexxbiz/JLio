namespace JLio.Extensions.Math.Builders;

public static class SumIfBuilders
{
    public static SumIf SumIf(params string[] arguments)
    {
        return new SumIf(arguments);
    }
}
