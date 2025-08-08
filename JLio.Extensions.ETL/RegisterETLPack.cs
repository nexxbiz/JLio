using JLio.Core.Contracts;
using JLio.Extensions.ETL.Commands;

namespace JLio.Extensions.ETL;

public static class RegisterETLPack
{
    public static IParseOptions RegisterETL(this IParseOptions parseOptions)
    {
        parseOptions.RegisterCommand<Resolve>();
        parseOptions.RegisterCommand<Flatten>();
        parseOptions.RegisterCommand<Restore>();
        parseOptions.RegisterCommand<ToCsv>();
        return parseOptions;
    }
}