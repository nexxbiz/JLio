using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TLio.Contracts;
using TLio.Contracts.DataFetcher;
using TLio.Contracts.Mutator;
using TLio.Models;
using TLio.Options;
using TLio.Services;
using TLio.Services.DataFetcher;
using TLio.Services.Mutator;

namespace TLio.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLio(this IServiceCollection services)
        {
            services.AddOptions<ScriptOptions>();

            return services
                   // .AddSingleton<IScriptRunner, ScriptRunner>()

                    //notificators
                    .AddMediatR(mediatr => mediatr.AsScoped(), typeof(ICommand), typeof(Command))

                    // Data Fetchers.
                    .AddSingleton<IDataFetcherRegistry, DataFetcherRegistry>()
                    
                    //Mutators
                    .AddSingleton<IMutatorRegistry, MutatorRegistry>()
                
                    //Add default data fetcher and mutator
                    .AddDataFetcher<DefaultFetcher>(ScriptOptions.DataEcosystemName)
                    .AddMutator<DefaultMutator>(ScriptOptions.DataEcosystemName)
                ;
        }


        public static IServiceCollection AddDataFetcher<TType>(this IServiceCollection services, string name)
            where TType : class
        {
            if (!typeof(TType).GetInterfaces().Contains(typeof(IDataFetcher))){ throw new NotImplementedException($"Registrating {nameof(TType)} failed because it needs to implement {nameof(IDataFetcher)}."); }
            // Register handler with DI.
            services.AddSingleton<TType>();

            // Register handler with options.
            services.Configure<ScriptOptions>(scriptOptions => scriptOptions.RegisterDataFetcher<TType>(name));

            return services;
        }
        
        public static IServiceCollection AddMutator<TType>(this IServiceCollection services, string name)
            where TType : class
        {
            if (!typeof(TType).GetInterfaces().Contains(typeof(IMutator))) { throw new NotImplementedException($"Registrating {nameof(TType)} failed because it needs to implement {nameof(IMutator)}."); }
            // Register handler with DI.
            services.AddSingleton<TType>();

            // Register handler with options.
            services.Configure<ScriptOptions>(scriptOptions => scriptOptions.RegisterMutator<TType>(name));

            return services;
        }
    }
}