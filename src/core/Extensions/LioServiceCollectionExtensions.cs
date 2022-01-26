using System;
using Lio.Core.Contracts;
using Lio.Core.Logs;
using Lio.Core.Options;
using Lio.Core.Runner;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

namespace Lio.Core.Extensions;

public static class LioServiceCollectionExtensions
{
    public static IServiceCollection AddLioCore(this IServiceCollection services,
        Action<LioOptionsBuilder>? configure = default)
    {
        var optionsBuilder = new LioOptionsBuilder(services);
        configure?.Invoke(optionsBuilder);
        RegisterLioServices(services, optionsBuilder.LioOptions);
        return services;
    }

    public static IServiceCollection AddLioCore(this IServiceCollection services, LioOptions options)
    {
        RegisterLioServices(services, options);
        return services;
    }

    private static void RegisterLioServices(IServiceCollection services, LioOptions options)
    {
        RegisterFetcherServices(services, options);
        RegisterMutatorServices(services, options);

        services.AddScoped<IScriptRunner, ScriptRunner>();
        services
            .TryAddSingleton<IClock>(SystemClock.Instance);
        services.AddMediatR(mediatr => mediatr.AsScoped(), typeof(ICommand), typeof(CommandExecutionLogWriter));
    }

    private static void RegisterFetcherServices(IServiceCollection services, LioOptions options)
    {
        services.AddSingleton(options.Fetcher);
        services.AddSingleton(sp => (ISpecificFetcher)sp.GetRequiredService(options.Fetcher));
    }

    private static void RegisterMutatorServices(IServiceCollection services, LioOptions options)
    {
        services.AddSingleton(options.Mutator);
        services.AddSingleton(sp => (ISpecificMutator)sp.GetRequiredService(options.Mutator));
    }
}