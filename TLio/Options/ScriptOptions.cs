namespace TLio.Options
{
    public class ScriptOptions
    {
        public static string DataEcosystemName = "Default";

        public ScriptOptions()
        {
            DataFetchers = new Dictionary<string, Type>();
            Mutators = new Dictionary<string, Type>();
        }

        public IDictionary<string, Type> DataFetchers { get; }
        public IDictionary<string, Type> Mutators { get; }
        
        public ScriptOptions RegisterMutator(Type mutator, string name)
        {
            //todo: what is the name already exisit? is the default error clear enough? should it override?
            Mutators.Add(name, mutator);
            return this;
        }

        public ScriptOptions RegisterDataFetcher(Type dataFetcher, string name)
        {
            //todo: what is the name already exisit? is the default error clear enough? should it override?
            DataFetchers.Add(name, dataFetcher);
            return this;
        }

        public ScriptOptions RegisterDataFetcher<TDataFetcher>(string name) =>
            RegisterDataFetcher(typeof(TDataFetcher), name);
        
        public ScriptOptions RegisterMutator<TMutator>(string name) =>
            RegisterMutator(typeof(TMutator), name);

    }
}