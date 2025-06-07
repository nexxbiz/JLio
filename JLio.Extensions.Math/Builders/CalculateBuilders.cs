namespace JLio.Extensions.Math.Builders;

public static class CalculateBuilders
{
    public static Calculate Calculate(string expression)
    {
        return new Calculate(expression);
    }
}
