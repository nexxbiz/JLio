namespace JLio.Extensions.TimeDate.Builders;

public static class MinDateBuilders
{
    public static MinDate MinDate(params string[] arguments)
    {
        return new MinDate(arguments);
    }
}