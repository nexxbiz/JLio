using System;

namespace TLio.Contracts.Mutator
{
    public interface IMutatorRegistry
    {
        void Register(Type mutator, string name);

        public IMutator GetMutator(string type);
    }
}