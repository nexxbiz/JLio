namespace JLio.Extensions.TimeDate.Builders;

public static class DateCompareBuilders
{
    public static DateCompare DateCompare(string date1, string date2)
    {
        return new DateCompare(date1, date2);
    }
}