using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TLio.Contracts.Mutator;
using TLio.Options;

namespace TLio.Services.Mutator
{
    public class MutatorRegistry : IMutatorRegistry
    {
        private readonly IServiceProvider serviceProvider;
        private IDictionary<string, Type> mutators { get; }

        public MutatorRegistry(IServiceProvider serviceProvider, IOptions<ScriptOptions> options)
        {
            this.serviceProvider = serviceProvider;
            mutators = new Dictionary<string, Type>(options.Value.Mutators);
        }

        public void Register(Type mutator, string name) => mutators.Add(name, mutator);

        public IMutator GetMutator(string type)
        {
            if (!mutators.ContainsKey(type))
            {
                return null;
            }

            return (IMutator)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, mutators[type]);
        }
    }
}