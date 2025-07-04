﻿using JLio.Core.Contracts;

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
        return parseOptions;
    }
}
