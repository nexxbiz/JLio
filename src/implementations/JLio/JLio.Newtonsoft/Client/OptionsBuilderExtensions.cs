using JLio.Newtonsoft.Fetcher;
using JLio.Newtonsoft.Mutators;
using Lio.Core.Options;

namespace JLio.Newtonsoft.Client
{
    public static class OptionsBuilderExtensions
    {
        public static LioOptionsBuilder WithNewtonsoft(this LioOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .WithMutator<NewtonsoftMutator>()
                .WithFetcher<JsonPathFetcher>();

            return optionsBuilder;
        }
    }
}