using System;
using Lio.Core.Contracts;

namespace Lio.Core.Options;

public class LioOptions
{
    public string ContainerName { get; set; } = Environment.MachineName;
    public Type Fetcher { get; set; }

    public Type Mutator { get; set; }

    // public ServiceFactory<ISpecificMutator> MutatorFactory { get; } = new();
}

public static class LioOptionsExtensions
{
    public static LioOptionsBuilder WithMutator(this LioOptionsBuilder optionsBuilder, Type mutator)
    {
        optionsBuilder.LioOptions.Mutator = mutator;
        return optionsBuilder;
    }

    public static LioOptionsBuilder WithMutator<T>(this LioOptionsBuilder optionsBuilder) where T : ISpecificMutator
    {
        optionsBuilder.LioOptions.Mutator = typeof(T);
        return optionsBuilder;
    }

    public static LioOptionsBuilder WithFetcher(this LioOptionsBuilder optionsBuilder, Type fetcher)
    {
        optionsBuilder.LioOptions.Fetcher = fetcher;
        return optionsBuilder;
    }

    public static LioOptionsBuilder WithFetcher<T>(this LioOptionsBuilder optionsBuilder) where T : ISpecificFetcher
    {
        optionsBuilder.LioOptions.Mutator = typeof(T);
        return optionsBuilder;
    }
}