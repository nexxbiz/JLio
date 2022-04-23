using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TLio.Contracts.DataFetcher;
using TLio.Options;

namespace TLio.Services.DataFetcher
{
    public class DataFetcherRegistry : IDataFetcherRegistry
    {
        private readonly IServiceProvider serviceProvider;
        
        private IDictionary<string, Type> fetchers { get; }

        public DataFetcherRegistry(IServiceProvider serviceProvider, IOptions<ScriptOptions> options)
        {
            this.serviceProvider = serviceProvider;
            fetchers = new Dictionary<string, Type>(options.Value.DataFetchers);
        }

        public void Register(Type fetcher, string name) => fetchers.Add(name, fetcher);

        public IDataFetcher GetFetcher(string type)
        {
            if (!fetchers.ContainsKey(type))
            {
                return null;
            }

            return (IDataFetcher)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, fetchers[type]);
        }
    }
}