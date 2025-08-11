namespace JLio.Extensions.TimeDate.Builders;

public static class AvgDateBuilders
{
    public static AvgDate AvgDate(params string[] arguments)
    {
        return new AvgDate(arguments);
    }
}