namespace JLio.Extensions.Math.Builders;

public static class ModuloBuilders
{
    public static Modulo Modulo(string dividend, string divisor)
    {
        return new Modulo(dividend, divisor);
    }
}
