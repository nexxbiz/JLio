namespace JLio.Extensions.Math.Builders;

public static class PowBuilders
{
    public static Pow Pow(string baseValue, string exponent)
    {
        return new Pow(baseValue, exponent);
    }
}