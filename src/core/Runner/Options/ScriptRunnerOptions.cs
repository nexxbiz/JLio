using System;
using Lio.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Lio.Core.Runner.Options
{
    public class ScriptRunnerOptions
    {
        internal ScriptRunnerOptions()
        {
            Mutator = sp => ActivatorUtilities.CreateInstance<ISpecificMutator>(sp);
            ConfigureSpecificMutator = (sp, mutatorConfig) => { };
        }

        internal Func<IServiceProvider, ISpecificMutator> Mutator { get; set; }

        internal Action<ServiceProvider, MutatorConfiguration> ConfigureSpecificMutator { get; set; }
    }
}