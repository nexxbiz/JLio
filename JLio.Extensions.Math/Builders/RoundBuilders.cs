namespace JLio.Extensions.Math.Builders;

public static class RoundBuilders
{
    public static Round Round(string value)
    {
        return new Round(value);
    }

    public static Round Round(string value, string decimals)
    {
        return new Round(value, decimals);
    }
}