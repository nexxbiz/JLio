using JLio.Core.Contracts;

namespace JLio.Extensions.TimeDate;

public static class RegisterTimeDatePack
{
    public static IParseOptions RegisterTimeDate(this IParseOptions parseOptions)
    {
        parseOptions.RegisterFunction<MaxDate>();
        parseOptions.RegisterFunction<MinDate>();
        parseOptions.RegisterFunction<AvgDate>();
        parseOptions.RegisterFunction<IsDateBetween>();
        parseOptions.RegisterFunction<DateCompare>();
        return parseOptions;
    }
}