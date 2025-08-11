using JLio.Core.Contracts;

namespace JLio.Extensions.Math;

public static class RegisterMathPack
{
    public static IParseOptions RegisterMath(this IParseOptions parseOptions)
    {
        parseOptions.RegisterFunction<Sum>();
        parseOptions.RegisterFunction<Avg>();
        parseOptions.RegisterFunction<Count>();
        parseOptions.RegisterFunction<Calculate>();
        parseOptions.RegisterFunction<Subtract>();
        parseOptions.RegisterFunction<Min>();
        parseOptions.RegisterFunction<Max>();
        parseOptions.RegisterFunction<Abs>();
        parseOptions.RegisterFunction<Round>();
        parseOptions.RegisterFunction<Floor>();
        parseOptions.RegisterFunction<Ceiling>();
        parseOptions.RegisterFunction<Pow>();
        parseOptions.RegisterFunction<Sqrt>();
        parseOptions.RegisterFunction<Median>();
        return parseOptions;
    }
}
