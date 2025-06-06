using JLio.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JLio.Extensions.Math
{
    public static class RegisterMathPack
    {
        public static IParseOptions RegisterMath(this IParseOptions parseOptions)
        {
            parseOptions.RegisterFunction<Sum>();
            parseOptions.RegisterFunction<Avg>();
            parseOptions.RegisterFunction<Count>();
            return parseOptions;
        }
    }
}
