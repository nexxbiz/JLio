namespace TLio.Contracts.Mutator
{
    public interface IMutatorRegistry<T>
    {
        void Register(Type mutator, string name);

        public IMutator<T> GetMutator(string type);
    }
}