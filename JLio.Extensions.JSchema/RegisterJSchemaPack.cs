using JLio.Core.Contracts;

namespace JLio.Extensions.JSchema;

public static class RegisterJSchemaPack
{
    public static IParseOptions RegisterJSchema(this IParseOptions parseOptions)
    {
        parseOptions.RegisterFunction<FilterBySchema>();
        return parseOptions;
    }
}