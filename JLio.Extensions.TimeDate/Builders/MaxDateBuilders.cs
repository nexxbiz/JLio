namespace JLio.Extensions.TimeDate.Builders;

public static class MaxDateBuilders
{
    public static MaxDate MaxDate(params string[] arguments)
    {
        return new MaxDate(arguments);
    }
}