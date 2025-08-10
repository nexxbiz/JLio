namespace JLio.Extensions.Math.Builders;

public static class MedianBuilders
{
    public static Median Median(params string[] arguments)
    {
        return new Median(arguments);
    }
}