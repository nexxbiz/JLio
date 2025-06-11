using JLio.Core.Contracts;

namespace JLio.Extensions.Text;

public static class RegisterTextPack
{
    public static IParseOptions RegisterText(this IParseOptions parseOptions)
    {
        parseOptions.RegisterFunction<Concat>();
        parseOptions.RegisterFunction<Format>();
        parseOptions.RegisterFunction<NewGuid>();
        parseOptions.RegisterFunction<Parse>();
        parseOptions.RegisterFunction<ToString>();
        return parseOptions;
    }
}
