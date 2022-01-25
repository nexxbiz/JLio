using System;
using System.Collections.Generic;
using System.Linq;
using Lio.Core.Contracts;
using Lio.Core.Runner.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Lio.Core.Options
{
    public class ServiceFactory<TService>
    {
        private readonly IDictionary<Type, Func<IServiceProvider, TService>> _dictionary = new Dictionary<Type, Func<IServiceProvider, TService>>();

        public IReadOnlyCollection<Type> Types => _dictionary.Keys.ToList().AsReadOnly();
        public void Add(Type type, Func<IServiceProvider, TService> factory) => _dictionary[type] = factory;
        public void Add(Type type, Func<TService> factory) => _dictionary[type] = _ => factory();
        public void Add(Type type, TService instance) => _dictionary[type] = _ => instance;

        public void Remove(Type type)
        {
            if (_dictionary.ContainsKey(type))
                _dictionary.Remove(type);
        }
        
        public TService CreateService(Type type, IServiceProvider scope) => _dictionary[type].Invoke(scope);
        public IEnumerable<TService> CreateServices(IServiceProvider scope) => _dictionary.Values.Select(factory => factory(scope));
    }
    
    public class LioOptions
    {
        internal LioOptions()
        {
            //TODO
            Mutator = typeof(ISpecificMutator);
        }
        public string ContainerName { get; set; } = Environment.MachineName;
        
        public Type Mutator { get; set; }
        
        public ServiceFactory<ISpecificMutator> MutatorFactory { get; } = new();


    }
    
    public class LioOptionsBuilder
    {
        private LioOptions LioOptions { get; }

        public LioOptionsBuilder(IServiceCollection serviceCollection)
        {
            LioOptions = new LioOptions();
            Services = serviceCollection;
        }
        
        public IServiceCollection Services { get; }
        
        
        public LioOptionsBuilder AddMutator(Type mutatorType)
        {
            var mutatorFactory = LioOptions.MutatorFactory;

            if (mutatorFactory.Types.Contains(mutatorType))
                return this;

            Services.AddSingleton(mutatorType);
            Services.AddSingleton(sp => (ISpecificMutator)sp.GetRequiredService(mutatorType));
            mutatorFactory.Add(mutatorType, provider => (ISpecificMutator)ActivatorUtilities.GetServiceOrCreateInstance(provider, mutatorType));
            return this;
        }
    }
}