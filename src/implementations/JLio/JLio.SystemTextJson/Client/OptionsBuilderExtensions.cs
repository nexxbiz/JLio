using JLio.JsonPath.Net.Fetcher;
using JLio.SystemTextJson.Mutators;
using Lio.Core.Options;

namespace JLio.SystemTextJson.Client
{
    public static class JlioSystemTextJson
    {
        public static LioOptions Options()
        {
            return new LioOptions
            {
                Mutator = typeof(SystemTextJsonMutator),
                Fetcher = typeof(JsonPathFetcher) // todo: is a project dependency needs to be a packagedependency
            };
        }
    }

    public static class OptionsBuilderExtensions
    {
        public static LioOptionsBuilder WithSystemTextJson(this LioOptionsBuilder optionsBuilder)
        {
            optionsBuilder.WithMutator<SystemTextJsonMutator>();
            optionsBuilder.WithFetcher<JsonPathFetcher>();
            return optionsBuilder;
        }
    }
}