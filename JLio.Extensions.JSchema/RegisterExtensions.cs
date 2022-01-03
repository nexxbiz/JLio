using JLio.Core.Contracts;

namespace JLio.Extensions.JSchema
{
    public static class RegisterExtensions
    {
        public static IFunctionsProviderRegistrar RegisterJSchemaFunctions(this IFunctionsProviderRegistrar parseOptions)
        {
            return parseOptions.Register<FilterBySchema>();
        }
        
        public static IFunctionsProviderRegistrar RegisterFilterByJSchemaFunction(this IFunctionsProviderRegistrar parseOptions)
        {
            return parseOptions.Register<FilterBySchema>();
        }
    }
}