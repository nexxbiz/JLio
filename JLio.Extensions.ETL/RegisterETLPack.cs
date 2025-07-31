using JLio.Core.Contracts;
using JLio.Extensions.ETL.Commands;

namespace JLio.Extensions.ETL;

public static class RegisterETLPack
{
    public static IParseOptions RegisterETL(this IParseOptions parseOptions)
    {
        parseOptions.RegisterCommand<Resolve>();
        return parseOptions;
    }
}