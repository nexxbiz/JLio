using JLio.Functions;

namespace JLio.Functions.Builders;

public static class DatetimeBuilders
{
    public static Datetime Datetime(params string[] arguments)
    {
        return new Datetime(arguments);
    }
}
