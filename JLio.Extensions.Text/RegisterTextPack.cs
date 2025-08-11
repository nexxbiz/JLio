using JLio.Core.Contracts;

namespace JLio.Extensions.Text;

public static class RegisterTextPack
{
    public static IParseOptions RegisterText(this IParseOptions parseOptions)
    {
        // Existing functions
        parseOptions.RegisterFunction<Concat>();
        parseOptions.RegisterFunction<Format>();
        parseOptions.RegisterFunction<NewGuid>();
        parseOptions.RegisterFunction<Parse>();
        parseOptions.RegisterFunction<ToString>();
        
        // New string manipulation functions
        parseOptions.RegisterFunction<Length>();
        parseOptions.RegisterFunction<Substring>();
        parseOptions.RegisterFunction<Replace>();
        parseOptions.RegisterFunction<IndexOf>();
        
        // Trimming functions
        parseOptions.RegisterFunction<Trim>();
        parseOptions.RegisterFunction<TrimStart>();
        parseOptions.RegisterFunction<TrimEnd>();
        
        // Case conversion functions
        parseOptions.RegisterFunction<ToUpper>();
        parseOptions.RegisterFunction<ToLower>();
        
        // Testing functions
        parseOptions.RegisterFunction<Contains>();
        parseOptions.RegisterFunction<StartsWith>();
        parseOptions.RegisterFunction<EndsWith>();
        parseOptions.RegisterFunction<IsEmpty>();
        
        // Array/string functions
        parseOptions.RegisterFunction<Split>();
        parseOptions.RegisterFunction<Join>();
        
        // Padding functions
        parseOptions.RegisterFunction<PadLeft>();
        parseOptions.RegisterFunction<PadRight>();
        
        return parseOptions;
    }
}
