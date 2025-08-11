namespace JLio.Extensions.TimeDate.Builders;

public static class IsDateBetweenBuilders
{
    public static IsDateBetween IsDateBetween(string checkDate, string startDate, string endDate)
    {
        return new IsDateBetween(checkDate, startDate, endDate);
    }
}